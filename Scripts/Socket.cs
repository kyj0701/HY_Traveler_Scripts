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

    private void ConnectToTcpServer()
    {
        try
        {
            socketConnection = new TcpClient("166.104.246.62", 17000);
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
            state = 3;
        }
    }

    /// Send message to server using socket connection.     
    private void SendMessage(Byte[] buffer)
    {
        if (socketConnection == null)
        {
            ConnectToTcpServer();
            // return;
        }
        try
        {
            // Get a stream object for writing.             
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                // Write byte array to socketConnection stream.                 
                stream.Write(buffer, 0, buffer.Length);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
            state = 3;
        }
        catch (Exception e)
        {
            Debug.Log("Exception : " + e);
            state = 4;
        }
    }

    private void RecvMessage()
    {
        try
        {
            if (toggle == 1)
            {
                Debug.Log("Receive Start!");
                // Get a stream object for writing.             
                NetworkStream stream = socketConnection.GetStream();
                // Debug.Log("Receiving " + stream);
                byte[] recvMessage = new Byte[4096];

                stream.Read(recvMessage, 0, recvMessage.Length);
                // Debug.Log("Receiving 1 " + recvMessage);

                traj = System.Text.Encoding.UTF8.GetString(recvMessage).ToString();
                if (traj.Equals("error"))
                {
                    state = 2;
                    Debug.Log("UnicodeDecodeError");
                }
                else
                {
                    // Debug.Log("Receiving 2 " + traj);
                    split_traj = traj.Split(' ');
                    // Debug.Log("Receiving 3 - tx: " + split_traj[3]);

                    qw = float.Parse(split_traj[0]);
                    qx = float.Parse(split_traj[1]);
                    qy = float.Parse(split_traj[2]);
                    qz = float.Parse(split_traj[3]);
                    tx = float.Parse(split_traj[4]);
                    ty = float.Parse(split_traj[5]);
                    tz = float.Parse(split_traj[6]);

                    Quaternion rot = new Quaternion(qx, qy, qz, qw);
                    Vector3 pos = new Vector3(tx, ty, tz);         

                    trajPos = Quaternion.Inverse(rot) * -pos;
                    trajPos = Vector3.Scale(trajPos, new Vector3(1,-1,1));
                    trajRot = Quaternion.Inverse(rot);
                    trajRot = new Quaternion(-trajRot.x, trajRot.y, -trajRot.z, trajRot.w);

                    transX.text = trajPos.x.ToString();
                    transY.text = trajPos.y.ToString();
                    transZ.text = trajPos.z.ToString();                

                    rotateX.text = trajRot.x.ToString();
                    rotateY.text = trajRot.y.ToString();
                    rotateZ.text = trajRot.z.ToString();
                    state = 0;
                }
            }
            else socketConnection = null;
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
        catch (Exception e)
        {
            Debug.Log("Exception : " + e);
            state = 4;
        }
    }

    public void SendToServerImage()
	{
        if (toggle == 1)
        {
            state = 1;
            Debug.Log("Task Start!");
            if (sendBtn.activeSelf == true) {
                sendBtn.SetActive(false);
            }
            mobileCamera = GameObject.Find("Camera Manager").GetComponent<CameraManager>();
            mobileCamera.ARCamera();
            CommunitcateWithServer(mobileCamera.m_LastCameraTexture.EncodeToPNG());
            t0 = new Vector3(arCamera.transform.position.x, arCamera.transform.position.y, arCamera.transform.position.z);
            q0 = arCamera.transform.rotation;
        }
        else socketConnection = null;
	}

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

    private void ConvertCoordinatesCOLMAPtoUnity (Vector3 pos, Quaternion rot)
    {
        trajPos = Quaternion.Inverse(rot) * -pos;
        trajPos = Vector3.Scale(trajPos, new Vector3(1,-1,1));
        trajRot = Quaternion.Inverse(rot);
        trajRot = new Quaternion(-trajRot.x, trajRot.y, -trajRot.z, trajRot.w);
    }
}
