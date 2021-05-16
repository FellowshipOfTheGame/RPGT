using Mirror;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    public List<Skill> skills = new List<Skill> { new BasicAttack(), new BasicHeal(), new Fireball() };
    public static CustomRoomPlayer localPlayerRoom = null;

    ImageMessage img = new ImageMessage();

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
        NetworkClient.RegisterHandler<ImageMessage>(ReceiveImg);
    }

    void SelectImg() {
        string path = EditorUtility.OpenFilePanel("Carregar imagem", "", "png");
        if (path.Length != 0)
        {
            img.img = File.ReadAllBytes(path);
            NetworkServer.SendToAll<ImageMessage>(img);
        }
    }

    void ReceiveImg(ImageMessage msg) {
        if (this.netId == msg.netId) {
            img = msg;
        }
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

            DrawPlayerUploadImgState();
            DrawPlayerUploadImg();
        }
    }

    void DrawPlayerUploadImgState()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 390f + (index * 100), 460f, 90f, 130f));

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
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 390f, 570f, 120f, 50f));

            if (GUILayout.Button("Upload img")) {
                SelectImg();
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

public struct ImageMessage : NetworkMessage
{
    public uint netId;
    public byte[] img;
    public bool hasLoaded { get { return img != null && img.Length != 0; } }
}