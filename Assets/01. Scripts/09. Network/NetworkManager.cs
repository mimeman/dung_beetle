using NetworkModels;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance { get { return instance; } }

    private SessionManager sessionManager;
    public SessionManager SessionManager { get { return sessionManager; } }

    private NetworkType networkType = NetworkType.None;

    public bool IsHost => networkType == NetworkType.None || networkType == NetworkType.Host;

    void Awake()
    {
        if (null != instance)
            Destroy(gameObject);

        instance = this;
        DontDestroyOnLoad(gameObject);

        sessionManager = GetComponent<SessionManager>();
    }

    void Start()
    {
    }
}