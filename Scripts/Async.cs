using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    // private static Client instance = null;
    public static Client Instance { get; set; }
    private TcpClient socketConnection;
    private CameraManager mobileCamera;
    public GameObject arCamera;
    public Text transX;
    public Text transY;
    public Text transZ;
    public Text rotateX;
    public Text rotateY;
    public Text rotateZ;
    public GameObject sendBtn;
    public Text mapText;

    private string traj;
    private string[] split_traj;
    private float qw, qx, qy, qz;
    private float tx, ty, tz;
    private Vector3 trajPos;
    private Quaternion trajRot;
    public Boolean connecting = false;
    private Vector3 t0;
    private Vector3 t1;
    private Quaternion q0;
    private Quaternion q1;

    public int state;
    public int toggle;

    private void Awake() 
    {
        Instance = this;
    }

    private void Start()
    {
        state = -1;
        toggle = 0;
        qw = 0;
        qx = 0;
        qy = 0;
        qz = 0;
        tx = 0;
        ty = 0;
        tz = 0;

    }

    private void Update() 
    {
        // if state = 0, server sends traj to client
        // if state = 2, client can't receive traj from server 
        // if state = 3, SocketException: Connection reset by peer or Connection failed
        // if state = 4, Exception: All exception
        if (state == 0 || state == 2 || state == 3 || state == 4) SendToServerImage();
        else if (state == -1) socketConnection = null;
    }

    private void ConnectToTcpServer() { }

    // Send message to server using socket connection.     
    private void SendMessage(Byte[] buffer) { }

    private void RecvMessage() { }

    public void SendToServerImage() { }

    async void CommunitcateWithServer(byte[] byteTestImageTexture)
    {
        await Task.Run(() =>
        {
            ConnectToTcpServer();

            string requestMessage =  "kyj0701 CameraImage.png " + byteTestImageTexture.Length;
            // string requestMessage =  GameManager.Instance.playerID + " CameraImage.png " + byteTestImageTexture.Length;
            // Debug.Log(requestMessage);

            SendMessage(System.Text.Encoding.UTF8.GetBytes(requestMessage));
            SendMessage(byteTestImageTexture);
            SendMessage(System.Text.Encoding.UTF8.GetBytes("EOF"));

            RecvMessage();

            {
                connecting = true;
                t1 = new Vector3(arCamera.transform.position.x, arCamera.transform.position.y, arCamera.transform.position.z);
                q1 = arCamera.transform.rotation;
                arCamera.transform.position = new Vector3(t1.x - t0.x + trajPos.x, t1.y - t0.y + trajPos.y, t1.z - t0.z + trajPos.z);
                arCamera.transform.rotation = (q1 * Quaternion.Inverse(q0)) * trajRot;

                // arCamera.transform.position = trajPos;
                // arCamera.transform.rotation = trajRot;
            }

            socketConnection = null;
        });
    }
}
