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

    private static Vector2 TMP_VECTOR2;

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
            cutSize = 0;
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
                TMP_VECTOR2 = CachedTran.anchoredPosition;                
                if (screenWidth > screenHeight)
                {
                    width -= cutSize;
                    TMP_VECTOR2.x = cutSize;
                }                    
                else
                {
                    screenHeight -= cutSize;
                    TMP_VECTOR2.y = -cutSize;
                }
                CachedTran.anchoredPosition = TMP_VECTOR2;
            }
            float standardCameraSize = standardHeight / 2.0f;
            float newAspect = width / height;
            float standardAspect = standardWidth / (float)standardHeight;
            float preSize = standardAspect / newAspect;
            if (newAspect < standardAspect)
            {
                uiCamera.orthographicSize = standardCameraSize * preSize;
                width = standardWidth;
                height = Math.Min(maxHeight, standardHeight * preSize);
            }
            else
            {
                uiCamera.orthographicSize = standardCameraSize;
                height = standardHeight;
                width = Math.Min(maxWidth, standardWidth / preSize);
            }
            TMP_VECTOR2.x = width;
            TMP_VECTOR2.y = height;
            CachedTran.sizeDelta = TMP_VECTOR2;
        }        
    }
}
