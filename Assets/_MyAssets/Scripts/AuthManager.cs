using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Firebase;
using Firebase.Auth;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase Data")]
    public FirebasePlayerInfo FirebasePlayer;

    //PlayerPref
    private string savedEmail;
    private string savedPassword;

    //Firebase
    [SerializeField] private DependencyStatus _dependentStatus;
    [SerializeField] private FirebaseAuth auth;
    [SerializeField] private FirebaseUser user;

    //Login
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField passwordField;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            _dependentStatus = task.Result;
            if (_dependentStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                FirebasePlayer.InitializeDatabase();
                Debug.Log("setting up Auth");
            }
            else
            {
                Debug.LogError("Could not resolve dependencies: " + _dependentStatus);
            }
        });
    }

    private void Start()
    {
        GetPlayerPrefs();
        StartCoroutine(signInDelay());
    }

    private void GetPlayerPrefs()
    {
        savedEmail = PlayerPrefs.GetString("email");
        savedPassword = PlayerPrefs.GetString("password");
    }

    private IEnumerator signInDelay()
    {
        yield return new WaitForSeconds(1);

        if (savedEmail != "" && savedPassword != "")
        {
            StartCoroutine(Login(savedEmail, savedPassword));
        }
        else
        {
            //loading.hide();
        }
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailField.text, passwordField.text));
    }

    private IEnumerator Login(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        //loading.show();

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null) // error message
        {
            Debug.Log(message: $"Failed task: {loginTask.Exception}");
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError error = (AuthError)firebaseException.ErrorCode;

            string message = "Failed";
            switch (error)
            {
                case AuthError.MissingEmail: message = "No Email"; break;
                case AuthError.MissingPassword: message = "No Password"; break;
                case AuthError.WrongPassword: message = "Wrong Password"; break;
                case AuthError.InvalidEmail: message = "Wrong Email"; break;
                case AuthError.UserNotFound: message = "User does not exist"; break;
                case AuthError.NetworkRequestFailed: message = "There's no internet"; break;
            }
            //NotificationScript.createNotif($"Failed to login: {message}", Color.red);

            //loading.hide();
        }
        else
        {
            user = loginTask.Result.User;
            if (user != null) FirebasePlayer.GetUser(user);

            Debug.Log($"Logged In with {email} and {password}");
            //NotificationScript.createNotif($"User {user.DisplayName} Signed in", Color.green);

            PlayerPrefs.SetString("email", email);
            PlayerPrefs.SetString("password", password);

            FirebasePlayer.LoadCloudData();
        }
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailField.text, passwordField.text, usernameField.text));
    }

    private IEnumerator Register(string email, string password, string username)
    {
        if (username == "")
        {
            //NotificationScript.createNotif($"Missing Username", Color.red);
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            //loading.show();

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.Log(message: $"Failed: {registerTask.Exception}");
                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseException.ErrorCode;

                string message = "Failed";
                switch (errorCode)
                {
                    case AuthError.MissingEmail: message = "Missing Email"; break;
                    case AuthError.MissingPassword: message = "Missing Password"; break;
                    case AuthError.WeakPassword: message = "Weak Password"; break;
                    case AuthError.EmailAlreadyInUse: message = "Email Already in use"; break;
                    case AuthError.NetworkRequestFailed: message = "There's no internet"; break;
                }
                //NotificationScript.createNotif($"Failed register: {message}", Color.red);
                //loading.hide();
            }
            else
            {
                user = registerTask.Result.User;
                if (user != null)
                {
                    FirebasePlayer.GetUser(user);

                    UserProfile profile = new UserProfile();
                    profile.DisplayName = username;

                    var profileTask = user.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                    if (profileTask.Exception != null)
                    {
                        Debug.Log(message: $"Failed: {profileTask.Exception}");
                        FirebaseException firebaseException = profileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError error = (AuthError)firebaseException.ErrorCode;
                        //NotificationScript.createNotif($"Username failed", Color.red);

                    }
                    else
                    {
                        //NotificationScript.createNotif($"You Registered!", Color.green);
                        StartCoroutine(FirebasePlayer.UpdateUsernameAuth(username));
                        StartCoroutine(FirebasePlayer.UpdateObject("username", username));
                        //GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>().setNickName(username);
                        //GameObject.FindGameObjectWithTag("Canvas").transform.Find("menu").GetComponent<menuScript>().UsernameField.text = username;
                        StartCoroutine(Login(email, password));
                    }
                }
            }
        }
    }

    public void SignOut()
    {
        auth.SignOut();
        usernameField.text = "";
        emailField.text = "";
        passwordField.text = "";

        SceneManager.LoadScene(0);
    }
}
