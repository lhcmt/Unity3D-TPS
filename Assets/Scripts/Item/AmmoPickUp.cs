using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUp : PickUpItems {


    public override void OnPickup(Collider collider)
    {
        base.OnPickup(collider);
        Container container = collider.gameObject.GetComponentInChildren<Container>();
        container.Add(itemInfo);


        if(!PhotonNetwork.connected)
        {
            Destroy(gameObject);
        }
        else 
        {
            PhotonNetwork.Destroy(gameObject);
        }

    }
}

