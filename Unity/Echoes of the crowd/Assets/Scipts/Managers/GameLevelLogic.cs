using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NPCsManager.Instance.LoadNPCs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
