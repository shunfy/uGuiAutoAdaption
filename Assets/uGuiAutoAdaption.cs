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
        [SerializeField, Header("该型号的刘海尺寸")]
        public float adaptionSize = 20f;
    }
    [SerializeField, Header("UI摄像机")]
    private Camera uiCamera;

    [SerializeField, Header("标准分辨率: 宽")]
    private int standardWidth = 1920;
    [SerializeField, Header("标准分辨率: 高")]
    private int standardHeight = 1080;

    [SerializeField, Header("最大自适应分辨率: 宽")]
    private int maxWidth = 1920 + 100;
    [SerializeField, Header("最大自适应分辨率: 高")]
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

    private static Vector2 TMP_VECTOR2;
    private static Vector3 TMP_VECTOR3;

    private RectTransform cachedTran;
    public RectTransform CachedTran { get { if (cachedTran == null) cachedTran = GetComponent<RectTransform>(); return cachedTran; } }

    public static uGuiAutoAdaption Inst { get; private set; }

    public float Width { get; private set; }
    public float Height { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (screenWidth != Screen.width || screenHeight != Screen.height)
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            float viewWidth = screenWidth;
            float viewHeight = screenHeight;

            #region 计算摄像机视野
            {
                float standardCameraSize = standardHeight / 2.0f;
                float standardAspect = standardWidth / (float)standardHeight;
                float screenAspect = viewWidth / viewHeight;
                float cameraScaleRatio = standardAspect / screenAspect;
                if (screenAspect < standardAspect)
                {
                    uiCamera.orthographicSize = standardCameraSize * cameraScaleRatio;
                    viewWidth = standardWidth;
                    viewHeight = standardHeight * cameraScaleRatio;
                }
                else
                {
                    uiCamera.orthographicSize = standardCameraSize;
                    viewHeight = standardHeight;
                    viewWidth = standardWidth / cameraScaleRatio;
                }
            }
            #endregion
            float adaptionX = 0;
            float adaptionY = 0;
            #region 计算自适应和刘海占用比
            {
                if (adaptions != null && adaptions.Length > 0)
                {
                    string deviceModel = getSysDeviceModel();
                    for (int i = 0; i < adaptions.Length; ++i)
                    {
                        if (deviceModel == adaptions[i].deviceModel)
                        {
                            if (screenWidth > screenHeight)
                                adaptionX = viewWidth * (adaptions[i].adaptionSize / screenWidth);                              
                            else
                                adaptionY = viewHeight * (adaptions[i].adaptionSize / screenHeight);
                            break;
                        }
                    }
                }
            }
            #endregion

            Width = viewWidth;
            Height = viewHeight;
            #region 计算实际使用大小
            {
                if (adaptionX > 0)
                {
                    Height = Math.Min(viewHeight, maxHeight);
                    if (viewWidth >= maxWidth + adaptionX * 2)
                    {
                        Width = maxWidth;
                    }
                    else if (viewWidth >= standardWidth + adaptionX * 2)
                    {
                        Width = viewWidth - adaptionX * 2;
                    }
                    else if (viewWidth >= standardWidth + adaptionX)
                    {
                        Width = viewWidth - adaptionX;
                        TMP_VECTOR2.x = adaptionX - (viewWidth - standardWidth) * 0.5f;
                        TMP_VECTOR2.y = 0;
                        CachedTran.anchoredPosition = TMP_VECTOR2;
                    }
                    else
                    {
                        TMP_VECTOR2.x = adaptionX - (viewWidth - standardWidth) * 0.5f;
                        TMP_VECTOR2.y = 0;
                        CachedTran.anchoredPosition = TMP_VECTOR2;

                        Width = standardWidth;
                        float tmp = standardWidth / (viewWidth - adaptionX);
                        Height = Math.Min(maxHeight, viewHeight * tmp);
                        TMP_VECTOR3 = Vector3.one * (1 / tmp);
                        CachedTran.localScale = TMP_VECTOR3;
                    }
                }
                else if (adaptionY > 0)
                {
                    Width = Math.Min(viewWidth, maxWidth);
                    if (viewHeight >= maxHeight + adaptionY * 2)
                    {
                        Height = maxHeight;
                    }
                    else if (viewHeight >= standardHeight + adaptionY * 2)
                    {
                        Height = viewHeight - adaptionY * 2;
                    }
                    else if (viewHeight >= standardHeight + adaptionY)
                    {
                        Height = viewHeight - adaptionY;
                        TMP_VECTOR2.y = (viewHeight - standardHeight) * 0.5f - adaptionY;
                        TMP_VECTOR2.x = 0;
                        CachedTran.anchoredPosition = TMP_VECTOR2;
                    }
                    else
                    {
                        TMP_VECTOR2.y = (viewHeight - standardHeight) * 0.5f - adaptionY;
                        TMP_VECTOR2.x = 0;
                        CachedTran.anchoredPosition = TMP_VECTOR2;

                        Height = standardHeight;
                        float tmp = standardHeight / (viewHeight - adaptionY);
                        Width = Math.Min(maxWidth, viewWidth * tmp);
                        TMP_VECTOR3 = Vector3.one * (1 / tmp);
                        CachedTran.localScale = TMP_VECTOR3;
                    }
                }
                else
                {
                    Width = Math.Min(viewWidth, maxWidth);
                    Height = Math.Min(viewHeight, maxHeight);
                }                
            }
            #endregion

            TMP_VECTOR2.x = Width;
            TMP_VECTOR2.y = Height;
            CachedTran.sizeDelta = TMP_VECTOR2;
        }        
    }
}
