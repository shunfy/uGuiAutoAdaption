using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uGuiAutoAdaption : MonoBehaviour
{
    [Serializable]
    private class Adaption
    {
        [SerializeField, Header("刘海设备型号")]
        public string deviceModel;
        [SerializeField, Header("该型号的刘海宽度")]
        public int cutWidth;
    }
    [SerializeField, Header("UI摄像机")]
    private Camera uiCamera;

    [SerializeField, Header("标准分辨率: 宽")]
    private int standardWidth = 1920;
    [SerializeField, Header("标准分辨率: 高")]
    private int standardHeight = 1080;

    [SerializeField, Header("最大自适应: 宽")]
    private int maxWidth = 1920 + 100;
    [SerializeField, Header("最大自适应: 高")]
    private int maxHeight = 1080 + 100;

    [SerializeField, Header("刘海屏适应列表")]
    private Adaption[] adaptions;

#if UNITY_EDITOR
    public string testSysDeviceModel = "test";
#endif

    private string getSysDeviceModel()
    {
#if UNITY_EDITOR
        return testSysDeviceModel;
#else
        return SystemInfo.deviceModel;
#endif
    }

    private int screenWidth;
    private int screenHeight;
    private int cutSize = 0;

    private RectTransform cachedTran;
    public RectTransform CachedTran { get { if (cachedTran == null) cachedTran = GetComponent<RectTransform>(); return cachedTran; } }

    // Start is called before the first frame update
    void Start()
    {
        if (uiCamera == null)
            uiCamera = GetComponent<Camera>();

        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (screenWidth != Screen.width || screenHeight != Screen.height)
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            string deviceModel = getSysDeviceModel();
            if (adaptions != null)
            {
                for(int i = 0; i < adaptions.Length; ++i)
                {
                    if (deviceModel == adaptions[i].deviceModel)
                    {
                        cutSize = adaptions[i].cutWidth;
                        break;
                    }
                }
            }
            float width = screenWidth;
            float height = screenHeight;
            if (cutSize != 0)
            {
                if (screenWidth > screenHeight)
                    width -= cutSize;
                else
                    screenHeight -= cutSize;
            }
            float standardCameraSize = standardHeight / 2.0f;
            float newAspect = width / height;
            float standardAspect = standardWidth / (float)standardHeight;
            float preSize = standardAspect / newAspect;
            if (newAspect < standardAspect)
            {
                uiCamera.orthographicSize = standardAspect * preSize;
                width = standardWidth;
                height = standardHeight * preSize;

            }
            else
            {
                uiCamera.orthographicSize = standardAspect;
            }
        }        
    }
}
