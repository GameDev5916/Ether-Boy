﻿using UnityEngine;
using UnityEngine.UI;
using Loom.Unity3d;
using Newtonsoft.Json;
using Google.Protobuf;
using System;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class backendClass : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    // Implemented in Assets/WebGL/settings.jslib
    [DllImport("__Internal")]
    private static extern string GetDAppChainConfigFromHostPage();
#endif

    public Identity identity;

    private Contract contract;
    
    [System.Serializable]
	private class envConfig {
		public string read_host;
		public string write_host;
	}

	private envConfig dAppChainCfg;

    private static envConfig LoadDAppChainConfig()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string cfgStr = GetDAppChainConfigFromHostPage();
#else
        string cfgStr = Resources.Load<TextAsset>("env_config").text;
#endif
        return JsonUtility.FromJson<envConfig>(cfgStr);
    }

    // Use this for initialization
    void Start()
    {
		if (dAppChainCfg == null) {
			//DontDestroyOnLoad (gameObject);
            dAppChainCfg = LoadDAppChainConfig();

			// By default the editor won't respond to network IO or anything if it doesn't have input focus,
			// which is super annoying when input focus is given to the web browser for the Auth0 sign-in.
			Application.runInBackground = true;
		}
    }

    private IAuthClient CreateAuthClient()
    {
#if !UNITY_WEBGL
	        try
	        {
	            CertValidationBypass.Enable();
	            return AuthClientFactory.Configure()
	                .WithLogger(Debug.unityLogger)
	                .WithClientId("25pDQvX4O5j7wgwT052Sh3UzXVR9X6Ud") // unity3d sdk
	                .WithDomain("loomx.auth0.com")
	                .WithScheme("io.loomx.unity3d")
	                .WithAudience("https://keystore.loomx.io/")
	                .WithScope("openid profile email picture")
	                .WithRedirectUrl("http://127.0.0.1:9998/auth/auth0/")
	                .Create();
	        }
	        finally
	        {
	            CertValidationBypass.Disable();
	        }
#else
	        return AuthClientFactory.Configure()
	            .WithLogger(Debug.unityLogger)
	            .WithHostPageHandlers(new Loom.Unity3d.WebGL.HostPageHandlers
	            {
	                SignIn = "authenticateFromGame",
	                GetUserInfo = "getUserInfo",
	                SignOut = "clearUserInfo"
	            })
	            .Create();
#endif
    }

#if !UNITY_WEBGL // In WebGL all interactions with the key store should be done in the host page.
	    private async Task<IKeyStore> CreateKeyStore(string accessToken)
	    {
	        return await KeyStoreFactory.CreateVaultStore(new VaultStoreConfig
	        {
	            Url = "https://stage-vault2.delegatecall.com/v1/",
	            VaultPrefix = "unity3d-sdk",
	            AccessToken = accessToken
	        });
	    }
#endif

	public void createIdentity () {
		//System.Text.Encoding.UTF8.GetString(this.identity.PrivateKey)
	}

	public async void SignIn()
    {
		if (dAppChainCfg == null) {
			Start ();
		}

		if (this.identity != null) {
			return;
		}
#if !UNITY_WEBGL
		if (PlayerPrefs.GetString("identityString") == "" && PlayerPrefs.GetString("usernameString") == "") {
	        try
	        {
	            CertValidationBypass.Enable();
	            var authClient = this.CreateAuthClient();
	            var accessToken = await authClient.GetAccessTokenAsync();
	            var keyStore = await this.CreateKeyStore(accessToken);
	            this.identity = await authClient.GetIdentityAsync(accessToken, keyStore);
	        }
	        finally
	        {
	            CertValidationBypass.Disable();
	        }
		} else {
			this.identity = new Identity{
				Username = PlayerPrefs.GetString("usernameString"),
				PrivateKey = CryptoUtils.HexStringToBytes(PlayerPrefs.GetString("identityString"))
			};
		}
#else
	        var authClient = this.CreateAuthClient();
	        this.identity = await authClient.GetIdentityAsync("", null);
#endif

		PlayerPrefs.SetString("identityString", CryptoUtils.BytesToHexString(this.identity.PrivateKey));
		PlayerPrefs.SetString ("usernameString", this.identity.Username);
         
        var writer = RPCClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithHTTP(dAppChainCfg.write_host)
            //.WithWebSocket("ws://etherboy-stage.loomapps.io/websocket")
            .Create();

        var reader = RPCClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithHTTP(dAppChainCfg.read_host)
            //.WithWebSocket("ws://etherboy-stage.loomapps.io/queryws")
            .Create();

        var client = new DAppChainClient(writer, reader)
        {
            Logger = Debug.unityLogger
        };
        client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]{
            new NonceTxMiddleware{
                PublicKey = this.identity.PublicKey,
                Client = client
            },
            new SignedTxMiddleware(this.identity.PrivateKey)
        });

        // There is only one contract address at the moment...
        var contractAddr = Address.FromHexString("0xe288d6eec7150D6a22FDE33F0AA2d81E06591C4d");
        var callerAddr = this.identity.ToAddress("default");
        this.contract = new Contract(client, contractAddr, callerAddr);

        //call create account, if it's a new user, an account will be created for Etherboy
		if (PlayerPrefs.GetInt ("hasAccount") == 0) {
			CreateAccount ();
		}
    }

    public async void SignOut()
    {
        var authClient = this.CreateAuthClient();
        await authClient.ClearIdentityAsync();
    }

    public async void ResetPrivateKey()
    {
#if !UNITY_WEBGL
	        try
	        {
	            CertValidationBypass.Enable();
	            var authClient = this.CreateAuthClient();
	            var accessToken = await authClient.GetAccessTokenAsync();
	            var keyStore = await this.CreateKeyStore(accessToken);
	            this.identity = await authClient.CreateIdentityAsync(accessToken, keyStore);
	        }
	        finally
	        {
	            CertValidationBypass.Disable();
	        }
#else
	        // TODO
	        throw new NotImplementedException();
#endif
    }

    // The backend doesn't care what the account info structure looks like,
    // it just needs to be serializable to JSON.
    // NOTE: Don't store any private info like email.
    private class SampleAccountInfo
    {
        public DateTime CreatedOn { get; set; }
    }

	public async void CreateAccount()
    {
        if (this.identity == null)
        {
            throw new System.Exception("Not signed in!");
        }

        // TODO: Check account doesn't exist before attempting to create one.

        // Create new player account
        var accountInfo = JsonConvert.SerializeObject(new SampleAccountInfo
        {
            CreatedOn = DateTime.Now
        });

        var createAcctTx = new EtherboyCreateAccountTx
        {
            Version = 0,
			Owner = this.identity.Username,
            Data = ByteString.CopyFromUtf8(accountInfo)
        };

		try {
        	await this.contract.CallAsync("CreateAccount", createAcctTx);
		} catch (Exception) {
			print ("Exception Caught");
			// account probably already exists for Etherboy
		} finally {
			PlayerPrefs.SetInt ("hasAccount", 1);
		}
    }

	public async void SaveState(SampleState saveData)
    {
        if (this.identity == null)
        {
            throw new System.Exception("Not signed in!");
        }

        // Save state to the backend
		var state = JsonConvert.SerializeObject(saveData);
        var saveStateTx = new EtherboyStateTx
        {
            Version = 0,
			Owner = this.identity.Username,
            Data = ByteString.CopyFromUtf8(state)
        };

        await this.contract.CallAsync("SaveState", saveStateTx);
    }

	public async void QuerySaveData()
    {
		try {
	        // NOTE: Query results can be of any type that can be deserialized via Newtonsoft.Json.
	        var result = await this.contract.StaticCallAsync<StateQueryResult>(
				"GetState", new StateQueryParams{ Owner = this.identity.Username }
	        );
				
			globalScript.loadGame(JsonConvert.DeserializeObject<SampleState>(result.State.ToStringUtf8()));
		} catch (Exception exception) {
			print (exception.Message);
			print ("Caught query exception");
		}
    }
}
