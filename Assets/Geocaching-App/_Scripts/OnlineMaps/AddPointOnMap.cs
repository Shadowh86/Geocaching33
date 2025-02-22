using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/AddPointOnMap")]
public class AddPointOnMap : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private GameObject userCanvas;
    [SerializeField] private GameObject adminCanvas;
    [SerializeField] private GameObject flagPrefab;
    [SerializeField] public TMP_Text addedMarkerText;
    [SerializeField] private GameObject markerLabelGameObject;
    [SerializeField] private TMP_InputField markerLabelInputField;
    [SerializeField] private Button createMarkerButton;
    double lng, lat;

    private List<int> index;
    private int number;
    private bool isCoroutineActive;
    private string labelText;
    private IEnumerator activeCoroutine;

    public PhotonView _photonView;
    private OnlineMapsMarkerManager _markerManager;

    private OnlineMapsLocationService _locationService;

    private OnlineMapsInteractiveElementManager<OnlineMapsMarkerManager, OnlineMapsMarker>
        _onlineMapsInteractiveElements;

    OnlineMapsMarker onlineMapsMarker;


    private void Start()
    {
        _onlineMapsInteractiveElements =
            FindObjectOfType<OnlineMapsInteractiveElementManager<OnlineMapsMarkerManager, OnlineMapsMarker>>();
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
        _photonView = PhotonView.Get(this);
        _locationService = FindObjectOfType<OnlineMapsLocationService>();

        number = 1;
    }

    public void RemoveMarker()
    {
        if (number == 1)
        {
            return;
        }

        if (number < 1)
        {
            _locationService.createMarkerInUserPosition = false;
            _locationService.createMarkerInUserPosition = true;
        }

        _photonView.RPC(nameof(RpcDeleteMarker), RpcTarget.AllBufferedViaServer);

    }

    public void OnMapClick()
    {
        if (adminCanvas.activeInHierarchy)
        {
            if (!markerLabelGameObject.activeInHierarchy)
            {
                markerLabelGameObject.SetActive(true);
                OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

                if (!markerLabelGameObject.activeInHierarchy)
                {
                    _markerManager.GetComponent<OnlineMapsMarkerManager>().enabled = true;
                }
                else if (markerLabelGameObject.activeInHierarchy)
                {
                    _markerManager.GetComponent<OnlineMapsMarkerManager>().enabled = false;
                }
            }
        }
    }

    public void OnClickCreateMarker()
    {
        isCoroutineActive = true;
        labelText = markerLabelInputField.text;
        string label = CreateLocationAndLabel();
        _photonView.RPC("RpcSendMessage", RpcTarget.AllBufferedViaServer, lng, lat, label);
        StartCoroutine(nameof(AddPointText));
        markerLabelInputField.text = String.Empty;
        Debug.Log("Photon view sent message!");
    }

    private IEnumerator AddPointText()
    {
        if (isCoroutineActive)
        {
            yield return new WaitForSeconds(5);
            addedMarkerText.text = "";
            isCoroutineActive = false;
        }
        else
        {
            StopCoroutine(nameof(AddPointText));
        }
    }


    string CreateLocationAndLabel()
    {
        // Create a label for the marker.
        string label = labelText;
        return label;
    }

    [PunRPC]
    private void RpcSendMessage(double lng, double lat, string label)
    {
        Debug.Log("Create marker"!);
        OnlineMapsMarkerManager.CreateItem(lng, lat, label);
        number++;
        //number = OnlineMapsMarkerManager.CountItems + 1;
        Debug.Log("Number: " + number);
        addedMarkerText.text = "You have added marker on: " + "\nLongitude: " + lng + "\nLangitude: " + lat;
    }

    [PunRPC]
    private void RpcDeleteMarker()
    {
        _onlineMapsInteractiveElements.Remove(OnlineMapsMarkerManager.RemoveItemAt(number), true);
        Debug.Log("Removed Marker!");
        number--;
        Debug.Log("Number: " + number);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
        }
        else
        {
            // Network player, receive data
            stream.ReceiveNext();
        }
    }
}