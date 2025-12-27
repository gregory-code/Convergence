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
    public Material Transition1Material;
    public Image Transition1Image;

    //PlayerPref
    private string savedEmail;
    private string savedPassword;

    //Firebase
    [SerializeField] private DependencyStatus _dependentStatus;
    [SerializeField] private FirebaseAuth auth;
    [SerializeField] private FirebaseUser user;

    private void Awake()
    {
        Transition1Image.gameObject.SetActive(true);
        Transition1Material.SetFloat("_Transition", 0);

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
            SignOut();
        }
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

            /*string message = "Failed";
            switch (error)
            {
                case AuthError.MissingEmail: message = "No Email"; break;
                case AuthError.MissingPassword: message = "No Password"; break;
                case AuthError.WrongPassword: message = "Wrong Password"; break;
                case AuthError.InvalidEmail: message = "Wrong Email"; break;
                case AuthError.UserNotFound: message = "User does not exist"; break;
                case AuthError.NetworkRequestFailed: message = "There's no internet"; break;
            }*/
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

            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float time = 0.0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime;
            Transition1Material.SetFloat("_Transition", time);
            yield return new WaitForEndOfFrame();
        }
        Transition1Image.gameObject.SetActive(false);
        Transition1Material.SetFloat("_Transition", 1);
    }

    public void SignOut()
    {
        auth.SignOut();
        SceneManager.LoadScene(1);
    }
}
