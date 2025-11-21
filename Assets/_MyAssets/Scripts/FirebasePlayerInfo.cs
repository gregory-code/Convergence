using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase;

using TMPro;
using UnityEngine.UI;
using System.Linq;

public class FirebasePlayerInfo : MonoBehaviour
{
    private List<IDataPersistence> dataPersistenceObjects;

    [Header("Firebase")]
    public DatabaseReference DataBaseReference;

    [SerializeField] private FirebaseUser user;

    public void Awake()
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    public void InitializeDatabase()
    {
        DataBaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void GetUser(FirebaseUser user) { this.user = user; }
    public string GetUserID() { return user.UserId; }

    public void LoadCloudData()
    {
        StartCoroutine(LoadData());
    }

    public IEnumerator UpdateUsernameAuth(string username)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = username;

        var profileTask = user.UpdateUserProfileAsync(profile);

        yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

        if (profileTask.Exception != null)
        {
            //NotificationScript.createNotif($"Failed to complete task: {profileTask.Exception}", Color.red);
        }
        else
        {
            //NotificationScript.createNotif($"Username is updated: {profileTask.Exception}", Color.green);
            //Auth username is now updated
        }
    }

    public IEnumerator UpdateObject(string ID, object value)
    {
        var dataBaseTask = DataBaseReference.Child("users").Child(user.UserId).Child(ID).SetValueAsync(value);

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            //NotificationScript.createNotif($"Failed to save {ID}: {dataBaseTask.Exception}", Color.red);
        }
        else { } // Data saved
    }



    private IEnumerator LoadData()
    {
        var dataBaseTask = DataBaseReference.Child("users").Child(user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        //loading.hide();
        //loginMenu.SetActive(false);

        if (dataBaseTask.Exception != null)
        {
            //NotificationScript.createNotif($"Failed to load data: {dataBaseTask.Exception}", Color.red);
        }
        else if (dataBaseTask.Result.Value == null)
        {
            // No content
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                StartCoroutine(dataPersistenceObj.LoadData(snapShot));
            }
        }
    }

    public IEnumerator LoadOtherPlayersData(string otherUserID, string ID)
    {
        var dataBaseTask = DataBaseReference.Child("users").Child(otherUserID).GetValueAsync();

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            //NotificationScript.createNotif($"Failed to load data: {dataBaseTask.Exception}", Color.red);
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadOtherPlayersData(ID, snapShot.Child(ID).Value);
            }
        }
    }

    private IEnumerator LoadAllDataTypeString(string toFind)
    {
        var dataBaseTask = DataBaseReference.Child("users").OrderByChild(toFind).GetValueAsync(); // gets all of the users based on highest kill count

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            //NotificationScript.createNotif($"Failed to load all {toFind}: {dataBaseTask.Exception}", Color.red);
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            foreach (Transform child in transform)
            {
                //example is for a scoreboard, you delete every existing prefab first
            }

            foreach (DataSnapshot childSnap in snapShot.Children.Reverse<DataSnapshot>())
            {
                //string username = childSnap.Child("username").Value.ToString();
                //int kills = int.Parse(childSnap.Child("Kills").Value.ToString());

                // instantiate the new scoreboard with all the data you want
            }
        }
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
