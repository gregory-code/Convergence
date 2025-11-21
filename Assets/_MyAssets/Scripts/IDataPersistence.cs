using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Firebase.Database;


public interface IDataPersistence
{
    IEnumerator LoadData(DataSnapshot data);

    void LoadOtherPlayersData(string key, object data);
}
