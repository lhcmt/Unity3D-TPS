using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CameraRig))]
public class CameraRigEditor : Editor {

    CameraRig cameraRig;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        cameraRig = (CameraRig)target;

        EditorGUILayout.LabelField("Camera Helper");

        if(GUILayout.Button("Save camPos"))
        {
            Camera cam = Camera.main;

            if(cam)
            {
                Transform camT = cam.transform;
                Vector3 camPos = camT.localPosition;
                Vector3 CamRight = camPos;
                Vector3 CamLeft = camPos;
                CamLeft.x = -camPos.x;
                cameraRig.cameraSettings.camPositionOffsetRight = CamRight;
                cameraRig.cameraSettings.camPositionOffsetLeft = CamLeft;
            }
        }
    }

}
