using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;
using UnityEngine.VR.WSA;

public class FieldAdjustmentMgr : MonoBehaviour
{

    public enum AdjustmentMode
    {
        Native,
        Translate,
        Rotate
    }

    private string[] SpeechActions =
    {
        "Adjust field", // start field adjustment
        "Cancel adjust", // cancel adjustment
        "Rotate", // switch to rotate mode
        "Translate", // switch to translate mode
        "Reset field offset", // reset the offset
        "Native", // Enables moving pins (default)
    };

    [SerializeField]
    private AdjustmentMode controllerType;
    private AdjustmentMode _execType;

    public Transform offsetTransform;
    public Transform pivotPoint;

    GestureRecognizer recongnizer;
    KeywordRecognizer wordRecognizer;

    public AudioSource commandSuccess;

    public TrackableLocation locationWithPoints;

    private void Start()
    {
        // create but don't start until in adjustment mode
        recongnizer = new GestureRecognizer();
        recongnizer.ManipulationStartedEvent += Recongnizer_ManipulationStartedEvent;
        recongnizer.ManipulationUpdatedEvent += Recongnizer_ManipulationUpdatedEvent;
        recongnizer.ManipulationCompletedEvent += Recongnizer_ManipulationCompletedEvent;
        recongnizer.ManipulationCanceledEvent += Recongnizer_ManipulationCanceledEvent;

        // create and start, listen for adjustment mode
        wordRecognizer = new KeywordRecognizer(SpeechActions);
        wordRecognizer.OnPhraseRecognized += WordRecognizer_OnPhraseRecognized;
        wordRecognizer.Start();

        lr = GetComponent<LineRenderer>();
    }

    public void AdjustField()
    {
        recongnizer.StartCapturingGestures();
        recongnizer.SetRecognizableGestures(GestureSettings.ManipulationTranslate);

        SetMode(AdjustmentMode.Native);
    }

    public void CancelAdjust()
    {
        recongnizer.StopCapturingGestures();
        locationWithPoints.SaveOffset();
    }

    public void ResetFieldOffset()
    {
        offsetTransform.localPosition = Vector3.zero;
        offsetTransform.localRotation = Quaternion.identity;
    }

    public void SetMode(AdjustmentMode type)
    {
        if(type == AdjustmentMode.Native)
        {
            if(locationWithPoints != null)
                locationWithPoints.visualizeAnchorPoints = true;
        }
        else if(controllerType == AdjustmentMode.Native && locationWithPoints != null)
        {
            locationWithPoints.visualizeAnchorPoints = false;
        }

        controllerType = type;
    }

#region Manipulation Gesture (translate/rotate)
    private LineRenderer lr;
    Vector3 startPos = Vector3.zero;
    Quaternion startRot = Quaternion.identity;
    Vector3 rotLocation = Vector3.zero;
    Transform selectedPoint = null;
    private void Recongnizer_ManipulationStartedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        startPos = offsetTransform.localPosition;
        startRot = offsetTransform.localRotation;

        Transform head = Camera.main.transform;

        rotLocation = (head.position + head.forward * 0.8f);

        // navtive only allowed if locationWithPoints is assigned
        if (controllerType == AdjustmentMode.Native && locationWithPoints == null) controllerType = AdjustmentMode.Translate;

        _execType = controllerType;

        if (_execType == AdjustmentMode.Native)
        {
            // cast ray
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 3, 1 << 26))
            {
                if(hit.collider)
                {
                    selectedPoint = hit.collider.transform.parent;
                    // reset starting pos
                    startPos = selectedPoint.localPosition;

                    DestroyImmediate(selectedPoint.GetComponent<WorldAnchor>());

                    if (lr != null)
                    {
                        lr.enabled = true;
                    }
                }
            }
        }
        else
        {
            if (lr != null)
            {
                lr.enabled = true;
            }
        }
    }

    private void Recongnizer_ManipulationUpdatedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (_execType == AdjustmentMode.Native)
        {
            // mov pin
            if (selectedPoint) selectedPoint.localPosition = startPos + cumulativeDelta;
        }
        else if (_execType == AdjustmentMode.Rotate)
        {
            var q = Quaternion.FromToRotation((rotLocation - pivotPoint.position), (rotLocation + cumulativeDelta - pivotPoint.position));
            offsetTransform.localRotation = q * startRot;
        }
        else
        {
            offsetTransform.localPosition = startPos + cumulativeDelta;
        }
        
        if(lr != null)
        {
            lr.SetPosition(0, rotLocation);
            lr.SetPosition(1, rotLocation + cumulativeDelta);
        }
    }

    private void Recongnizer_ManipulationCompletedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if (lr != null)
        {
            lr.enabled = false;
        }
        if (_execType == AdjustmentMode.Native && selectedPoint != null)
        {
            //var worldPoint = selectedPoint.transform.parent.gameObject;
            //DestroyImmediate(worldPoint.GetComponent<WorldAnchor>());
            //worldPoint.transform.position += cumulativeDelta;
            //selectedPoint.transform.localPosition = Vector3.zero;

            // save if we made changes to keep
            locationWithPoints.SaveLocation();

            selectedPoint = null;
        }
    }

    private void Recongnizer_ManipulationCanceledEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        if(_execType == AdjustmentMode.Native)
        {
            if(selectedPoint != null)
            {
                selectedPoint.localPosition = startPos;
                selectedPoint.gameObject.AddComponent<WorldAnchor>();
            }
            selectedPoint = null;
        }
        else
        {
            offsetTransform.localPosition = startPos;
            offsetTransform.localRotation = startRot;
        }
        if (lr != null)
        {
            lr.enabled = false;
        }
    }
    #endregion

    private void WordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        switch (System.Array.IndexOf<string>(SpeechActions, args.text))
        {
            case (0):
                if(!recongnizer.IsCapturingGestures())
                {
                    AdjustField();
                    if (commandSuccess) commandSuccess.Play();
                }
                break;
            case (1):
                if (recongnizer.IsCapturingGestures())
                {
                    CancelAdjust();
                    if (commandSuccess) commandSuccess.Play();
                }
                break;
            case (2):
                if (recongnizer.IsCapturingGestures())
                {
                    SetMode(AdjustmentMode.Rotate);
                    if (commandSuccess) commandSuccess.Play();
                }
                break;
            case (3):
                if (recongnizer.IsCapturingGestures())
                {
                    SetMode(AdjustmentMode.Translate);
                    if (commandSuccess) commandSuccess.Play();
                }
                break;
            case (4):
                if (recongnizer.IsCapturingGestures())
                {
                    ResetFieldOffset();
                    if (commandSuccess) commandSuccess.Play();
                }
                break;
            case (5):
                if (recongnizer.IsCapturingGestures())
                {
                    SetMode(AdjustmentMode.Native);
                    if (commandSuccess) commandSuccess.Play();
                }
                break;
            default:
                break;
        }
    }

#if UNITY_EDITOR
    // enable from the editor
    public bool startController = false;
    private void Update()
    {
        if(startController)
        {
            AdjustField();
            startController = false;
        }
    }
#endif
}
