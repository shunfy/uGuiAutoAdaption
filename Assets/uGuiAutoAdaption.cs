using System;
using UnityEngine;

[ExecuteInEditMode]
public class uGuiAutoAdaption : MonoBehaviour
{
    [Serializable]
    public enum AdaptionType
    {
        [Header("范围内自适应")]
        Auto = 1,
        [Header("范围内强拉伸")]
        Stretch = 2,
    }

    [Serializable]
    public class Adaption
    {
        [SerializeField, Header("设备型号")]
        public string deviceModel;
        [SerializeField, Header("适配分辨率: 宽"), Tooltip("不可以小于标准分辨率尺寸比")]
        public int targetX;
        [SerializeField, Header("适配分辨率: 高"), Tooltip("不可以小于标准分辨率尺寸比")]
        public int targetY;
        [SerializeField, Header("X轴偏移值")]
        public int offsetX;
        [SerializeField, Header("Y轴编译值")]
        public int offsetY;
    }
    [SerializeField, Header("UI摄像机")]
    private Camera uiCamera;

    [SerializeField, Header("分辨率适配方式")]
    private AdaptionType adaptionType = AdaptionType.Auto;

    [SerializeField, Header("标准分辨率: 宽")]
    private int standardWidth = 1920;
    [SerializeField, Header("标准分辨率: 高")]
    private int standardHeight = 1080;

    [SerializeField, Header("最大自适应分辨率: 宽")]
    private int maxWidth = 1920 + 100;
    [SerializeField, Header("最大自适应分辨率: 高")]
    private int maxHeight = 1080 + 100;

    [SerializeField, Header("特殊适配列表")]
    private Adaption[] adaptions;

    public void SetAdaption(AdaptionType _adaptionType, int _standardWidth, int _standardHeight, int _maxWidth, int _maxHeight)
    {
        adaptionType = _adaptionType;
        standardWidth = _standardWidth;
        standardHeight = _standardHeight;
        maxWidth = _maxWidth;
        maxHeight = _maxHeight;
    }

    public void ExecAdaption(int targetX, int targetY, int offsetX, int offsetY)
    {
        Adaption adaption = new Adaption();
        adaption.targetX = targetX;
        adaption.targetY = targetY;
        adaption.offsetX = offsetX;
        adaption.offsetY = offsetY;
        ExecAdaption(adaption);
    }

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

    private static Vector2 TMP_VECTOR2;
    private static Vector3 TMP_VECTOR3;

    private RectTransform cachedTran;
    public RectTransform CachedTran { get { if (cachedTran == null) cachedTran = GetComponent<RectTransform>(); return cachedTran; } }

    public float ScreenWidth { get; private set; }
    public float ScreenHeight { get; private set; }

    public float Width { get; private set; }
    public float Height { get; private set; }
    public Vector2 Size
    {
        get
        {
            TMP_VECTOR2.x = Width;
            TMP_VECTOR2.y = Height;
            return TMP_VECTOR2;
        }
    }
    public int StandardWidth { get { return standardWidth; } }
    public int StandardHeight { get { return standardHeight; } }
    public float ViewWidth { get; private set; }
    public float ViewHeight { get; private set; }

    /// <summary>
    /// 外部调用的自适应接口
    /// </summary>
    /// <param name="adaption">适配参数</param>
    public void ExecAdaption(Adaption adaption = null)
    {
        ScreenWidth = Screen.width;
        ScreenHeight = Screen.height;

        ViewWidth = ScreenWidth;
        ViewHeight = ScreenHeight;

        #region 计算摄像机视野
        {
            float standardCameraSize = standardHeight / 2.0f;
            float standardAspect = standardWidth / (float)standardHeight;
            float screenAspect = ViewWidth / ViewHeight;
            float cameraScaleRatio = standardAspect / screenAspect;
            if (screenAspect < standardAspect)
            {
                uiCamera.orthographicSize = standardCameraSize * cameraScaleRatio;
                ViewWidth = standardWidth;
                ViewHeight = standardHeight * cameraScaleRatio;
            }
            else
            {
                uiCamera.orthographicSize = standardCameraSize;
                ViewHeight = standardHeight;
                ViewWidth = standardWidth / cameraScaleRatio;
            }
        }
        #endregion

        Width = Math.Min(ViewWidth, maxWidth);
        Height = Math.Min(ViewHeight, maxHeight);

        #region 特殊适配自适应
        if (adaption != null)
        {
            Width = ViewWidth * (adaption.targetX / ScreenWidth);
            Height = ViewHeight * (adaption.targetY / ScreenHeight);

            TMP_VECTOR2.x = adaption.offsetX;
            TMP_VECTOR2.y = adaption.offsetY;
            CachedTran.anchoredPosition = TMP_VECTOR2;
        }
        #endregion

        if (adaptionType == AdaptionType.Stretch)
        {
            TMP_VECTOR3.x = Width / standardWidth;
            TMP_VECTOR3.y = Height / standardHeight;
            TMP_VECTOR3.z = 1;
            CachedTran.localScale = TMP_VECTOR3;

            Width = standardWidth;
            Height = standardHeight;
        }

        TMP_VECTOR2.x = Width;
        TMP_VECTOR2.y = Height;
        CachedTran.sizeDelta = TMP_VECTOR2;
    }

    void execAdaption()
    {
        if (adaptions != null && adaptions.Length > 0)
        {
            string deviceModel = getSysDeviceModel();
            for (int i = 0; i < adaptions.Length; ++i)
            {
                if (deviceModel == adaptions[i].deviceModel)
                {
                    ExecAdaption(adaptions[i]);
                    return;
                }
            }
        }

        ExecAdaption();
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        execAdaption();
    }

    private AdaptionType last_adaptionType = AdaptionType.Auto;
    void Update()
    {
        if (ScreenWidth != Screen.width || ScreenHeight != Screen.height || last_adaptionType != adaptionType)
        {
            last_adaptionType = adaptionType;
            execAdaption();
        }
    }
#endif
}
