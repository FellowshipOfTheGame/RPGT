using Mirror;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    public List<Skill> skills = new List<Skill> { new BasicAttack(), new BasicHeal(), new Fireball() };
    public static CustomRoomPlayer localPlayerRoom = null;

    CustomNetworkRoomManager customNetworkRoomManager;

    public ImageMessage img = new ImageMessage();
    public Texture2D texture;

    void Awake() {
        customNetworkRoomManager = NetworkManager.singleton as CustomNetworkRoomManager;
        texture = new Texture2D(0, 0);
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() {
        if (localPlayerRoom != null) {
            Debug.LogWarning("Houve uma tentativa de setar dois localPlayerRooms");
            Destroy(this);
        }
        localPlayerRoom = this;
        img.netId = this.netId;
    }

    public override void OnGUI()
    {
        if (!showRoomGUI)
            return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            DrawPlayerImage();
            DrawPlayerUploadImgState();
            DrawPlayerUploadImg();
        }
    }

    void DrawPlayerImage()
    {
        if (!img.hasLoaded)
            return;

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 390f + (index * 100), 460f, 90f, 90f), texture);
        GUILayout.EndArea();
    }

    void DrawPlayerUploadImgState()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 390f + (index * 100), 460f + 90f, 90f, 130f));

        GUILayout.Label($"Player [{index + 1}]");

        if (readyToBegin)
            GUILayout.Label("Ready");
        else
            GUILayout.Label("Not Ready");
        if (img.hasLoaded)
            GUILayout.Label("Imagem carregada");
        else
            GUILayout.Label("Imagem ainda não carregada");

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE"))
        {
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        GUILayout.EndArea();
    }

    void DrawPlayerUploadImg()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 390f, 570f + 90f, 120f, 50f));

            if (GUILayout.Button("Upload img")) {
                StartCoroutine( customNetworkRoomManager.SelectImageCoroutine(img, texture) );
            }
            if (readyToBegin)
            {
                if (GUILayout.Button("Cancel"))
                    CmdChangeReadyState(false);
            }
            else
            {
                if (GUILayout.Button("Ready"))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }
}