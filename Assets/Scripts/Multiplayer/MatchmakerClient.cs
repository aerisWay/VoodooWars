
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core.Environments;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class MatchmakerClient : MonoBehaviour
{
   
    private string ticketId;
    [SerializeField] GameObject matchmakingPanels;
    [SerializeField] GameObject fadePanel;

    private  void OnEnable()
    {
        
        ServerStartUp.ClientInstance += SignIn;         
       
    
    }

    private void OnDisable()
    {
        ServerStartUp.ClientInstance -= SignIn;
    }

    private async void SignIn()
    {
        print("Alo");
        await ClientSignIn("VoodooPlayer");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        StartClient();
    
    }

    private async Task ClientSignIn(string serviceProfileName = null)
    {
        if(serviceProfileName != null) 
        {
            #if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
            #endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
           
         
            await UnityServices.InitializeAsync(initOptions);
        }
        else
        {
          
            await UnityServices.InitializeAsync();

        }

        Debug.Log($"Signed In Anonymously as {serviceProfileName}({PlayerID()}");

        
    }

    private string PlayerID()
    {
        print("Get player ID");
        return AuthenticationService.Instance.PlayerId;
        
    }

#if UNITY_EDITOR
    private string GetCloneNumberSuffix()
    {
        {
            string projectPath = ClonesManager.GetCurrentProjectPath();
            int lastUnderscore = projectPath.LastIndexOf('_');
            string projectCloneSuffix = projectPath.Substring(lastUnderscore + 1);
            if (projectCloneSuffix.Length != 1)
                projectCloneSuffix = "";
            return projectCloneSuffix;
        }

    }

#endif

    public void StartClient()
    {
       
        CreateATicket();
    }

    private async void CreateATicket()
    {
      
        print("Client Started");
        matchmakingPanels.SetActive(true);
        fadePanel.SetActive(true);
        var options = new CreateTicketOptions("VoodooMode");
      
        var players = new List<Player>
        {
            new Player(
                PlayerID(),
                new MatchmakingPlayerData
                {
                    skill = 100
                }
                
            )
        };

        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        ticketId = ticketResponse.Id;
        Debug.Log($"Ticket ID: {ticketId}");
        PollTicketStatus();

       
    }

    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignment = false;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketId);
            if (ticketStatus == null) continue;
            if(ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch(multiplayAssignment.Status)
            {
                case StatusOptions.Found:
                    gotAssignment = true; 
                    TicketAssigned(multiplayAssignment);
                    matchmakingPanels.SetActive(false);
                    fadePanel.SetActive(false);
                    Debug.Log("Ticket found");
                    break;
                case StatusOptions.InProgress:
                    Debug.Log("Waiting for ticket");
                    break;
                case StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.Log("Failed to get ticket status");
                    matchmakingPanels.SetActive(false);
                    fadePanel.SetActive(false);
                    break;
                case StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.Log("Time out to get the ticket.");
                    matchmakingPanels.SetActive(false);
                    fadePanel.SetActive(false);
                    break;
                default:
                    throw new InvalidOperationException();
                
            }
        } while (!gotAssignment);
    }

    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log("Ticket assignment: " + assignment.Ip);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
        Debug.Log("Im a client.");
    }

    [Serializable]
    public class MatchmakingPlayerData
    {
        public int skill;
    }
}
