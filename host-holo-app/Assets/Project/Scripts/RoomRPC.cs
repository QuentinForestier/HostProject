using Host.Network;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;
using static Host.Network.HostNetworkManager;

public class RoomRPC : MonoBehaviour
{
    private static bool _rpcInitDone = false;

    private void InitRpc()
    {
        HostNetworkManager.RegisterGameObject(HostNetworkId, this);
        HostNetworkManager.RegisterRPC(HostNetworkId, 0, "HololensConnected");
        HostNetworkManager.RegisterRPC(HostNetworkId, 1, "DisplayHostname");
        HostNetworkManager.RegisterRPC(HostNetworkId, 2, "StartPlacement");
        HostNetworkManager.RegisterRPC(HostNetworkId, 3, "AnchorPlaced");
        HostNetworkManager.RegisterRPC(HostNetworkId, 4, "RoomReady");
        HostNetworkManager.RegisterRPC(HostNetworkId, 5, "StartSimulation");
        _rpcInitDone = true;
    }

    public ConsoleInfos StandardMessageConsole;

    //public GameObject PlacementMenu;

    public SetupSpatialPosition AirplaneManipulator;

    public GameObject MainGeometry;

    public TrainingMode TrainingMode;

    private bool IsPlacingGeometry = false;

    public int HostNetworkId = 2;

    void Awake()
    {
        if(!_rpcInitDone)
        {
            InitRpc();
        }

#if !UNITY_EDITOR
        MainGeometry.SetActive(false);
#endif
    }

    private void Start()
    {
        UnityEngine.WSA.Application.windowActivated += Application_windowActivated;
    }

    private void Application_windowActivated(UnityEngine.WSA.WindowActivationState state)
    {
        if (state == UnityEngine.WSA.WindowActivationState.CodeActivated)
        {
            //when back to app
            //restart unity app
        }
        else if (state == UnityEngine.WSA.WindowActivationState.Deactivated)
        {
            //when bloom gesture
        }
    }

    public void ValidateAnchorPlacement()
    {
        if (!IsPlacingGeometry)
        {
            return;
        }

        Debug.Log("[RoomRPC] - Sending Anchor Placed");
        HostNetwork.RPC(HostNetworkId, "AnchorPlaced", HostNetworkTarget.Server, HostNetwork.ClientLocalIP);

        // Disable anchor selection
        // PlacementMenu.SetActive(false);
        AirplaneManipulator.transform.parent.gameObject.SetActive(false);
        AirplaneManipulator.StopPlacing();

        IsPlacingGeometry = false;

        StandardMessageConsole.ShowMessage("Prêt - En attente de démarrage");
    }

    public void ShowEndScreen()
    {
        MainGeometry.SetActive(false);

        StandardMessageConsole.SetVisible(true);
        StandardMessageConsole.ShowMessage("Merci d'avoir participé !");
    }

    /// <summary>
    /// Room ready
    /// </summary>
    public void RoomReady()
    {
        Debug.Log("[RoomRPC] - RoomReady");

        Debug.Log("=> Telling master that we are connected");
        HostNetwork.RPC(HostNetworkId, "HololensConnected", HostNetworkTarget.Server, HostNetwork.ClientLocalIP);

        StandardMessageConsole.SetVisible(true);
        StandardMessageConsole.ShowMessage("Hololens ready and connected");
    }

    /// <summary>
    /// Display the hostname on the HoloLens
    /// </summary>
    public void DisplayHostname()
    {
        Debug.Log("[RoomRPC] - DisplayHostname");

        if (StandardMessageConsole != null)
        {
            StandardMessageConsole.SetVisible(true);
            StandardMessageConsole.ShowMessage(SystemInfo.deviceName);
        }
    }

    /// <summary>
    /// Called by a Hololens when it's connected to the room
    /// </summary>
    public void HololensConnected(string sender)
    {
        // not used here, only by master
    }

    /// <summary>
    /// Called by the Host when a Hololens needs to place the geometry
    /// </summary>
    public void StartPlacement()
    {
        Debug.Log("[RoomRPC] - StartPlacement");
        Debug.Log("=> Placement has been started on this hololens");

        StandardMessageConsole.SetVisible(true);
        StandardMessageConsole.ShowMessage("Veuillez positionner la scène");

        // Enable anchor selection
        MainGeometry.SetActive(true);
        TrainingMode.SetupPlacementMode();
        //PlacementMenu.SetActive(true);
        AirplaneManipulator.transform.parent.gameObject.SetActive(true);
        AirplaneManipulator.StartPlacing();

        IsPlacingGeometry = true;
    }

    public void AnchorPlaced(string sender)
    {
        // not used here
    }

    /// <summary>
    /// Start simulation
    /// </summary>
    public void StartSimulation()
    {
        Debug.Log("[RoomRPC] - StartSimulation");

        StandardMessageConsole.SetVisible(false);
        TrainingMode.SetupTrainingMode();
    }

}
