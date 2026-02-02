using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using XM;
using XM.Contracts;
using XM.Utils;

namespace XM
{
    public class UIStack
    {
        public UII Handle;
        public bool IsHideSelf;

        public bool IsHideByStack;

        // TODo
        public int CanvasLayerId;
    }

    public class LoadedUIData
    {
        public CfgI<UIConfigUnManaged> StaticConfigId;
        public GameObject UIPrefabTemplate;
        public UIConfig Config;
        public XAssetHandle AssetHandle;

        public IUICtrlBase IUICtrlTemplate;

        // 挪到UImnager中去 
        public readonly Dictionary<UII, UICtrlBase> ActiveUIIs = new();

        // 挪到UImnager中去 
        public readonly Dictionary<UII, (UICtrlBase ctrlBase, float closeTime)> WillReleaseUIIs = new();
        public EAssetStatus Status;
    }


    public class UIConfig
    {
    }


    public class UIWindowConfig : UIConfig
    {
        public CfgI UITypeI;
        public EUILayer Layer;
        public EUIInstanceType InstanceType;
        public bool IsShowMask;
    }

    public class UILayerData
    {
        public int StartLayerId;
        public int EndLayerId;

        public LinkedList<UIStack> UIStacks = new LinkedList<UIStack>();

        // Standalone类型UI的独立栈管理
        public List<UIStack> StandAloneStacks = new List<UIStack>();

        public GameObject LayerRoot;

        public Canvas RootCanvas;
    }

    /// <summary>
    /// UI管理器
    /// </summary>
    [AutoCreate]
    public class UIManager : ManagerBase<IUIManager>, IUIManager
    {
        public MultiKeyDictionary<AssetI, CfgI<UIConfigUnManaged>, LoadedUIData> loadedUIData = new();

        public Dictionary<EUILayer, UILayerData> UILayerData = new();

        public Camera UICamera;

        public GameObject UIRoot;

        // 全局UI实例ID计数器，确保所有组件使用统一的唯一ID
        private int _globalInstanceIdCounter = 0;

        public override UniTask OnCreate()
        {
            CreateUIRoot();
            FindAndSetupUICamera();
            InitializeUILayers();
            return UniTask.CompletedTask;
        }


        /// <summary>
        /// 创建UI根节点
        /// </summary>
        private void CreateUIRoot()
        {
            UIRoot = new GameObject("UIRoot");
            UIRoot.transform.SetParent(transform);
            UIRoot.transform.localPosition = Vector3.zero;
            UIRoot.transform.localRotation = Quaternion.identity;
            UIRoot.transform.localScale = Vector3.one;
            XLog.Info("创建 UIRoot 节点");
        }

        /// <summary>
        /// 查找并设置UI相机
        /// </summary>
        private void FindAndSetupUICamera()
        {
            GameObject uiCameraObj = GameObject.Find("UICamera");
            if (uiCameraObj != null)
            {
                UICamera = uiCameraObj.GetComponent<Camera>();
                if (UICamera == null)
                {
                    XLog.Warning("找到名为 'UICamera' 的对象，但没有 Camera 组件");
                    return;
                }

                // 移动UI相机到ManagerRoot
                UICamera.transform.SetParent(transform);
                XLog.Info("成功找到并设置 UI 相机");
            }
            else
            {
                XLog.Warning("未找到名为 'UICamera' 的对象");
            }
        }

        /// <summary>
        /// 初始化UI层级
        /// </summary>
        private void InitializeUILayers()
        {
            foreach (EUILayer layer in Enum.GetValues(typeof(EUILayer)))
            {
                if (layer == EUILayer.None)
                {
                    CreateNoneLayer();
                }
                else
                {
                    CreateUILayer(layer);
                }
            }
        }

        /// <summary>
        /// 创建None层级用于回收UI
        /// </summary>
        private void CreateNoneLayer()
        {
            // 创建None层级根节点（只是一个容器，不需要Canvas等组件）
            GameObject noneRoot = new GameObject("UILayer_None");
            noneRoot.transform.SetParent(UIRoot.transform);
            noneRoot.transform.localPosition = Vector3.zero;
            noneRoot.transform.localRotation = Quaternion.identity;
            noneRoot.transform.localScale = Vector3.one;
            noneRoot.SetActive(false); // 默认隐藏，回收的UI不可见

            // 存储层级数据
            UILayerData layerData = new UILayerData
            {
                LayerRoot = noneRoot,
                RootCanvas = null, // None层级不需要Canvas
                StartLayerId = 0,
                EndLayerId = 0
            };
            UILayerData[EUILayer.None] = layerData;

            XLog.Info("创建 UI 回收层级: None");
        }

        /// <summary>
        /// 创建单个UI层级
        /// </summary>
        private void CreateUILayer(EUILayer layer)
        {
            // 创建层级根节点
            GameObject layerRoot = new GameObject($"UILayer_{layer}");
            layerRoot.transform.SetParent(UIRoot.transform);
            layerRoot.transform.localPosition = Vector3.zero;
            layerRoot.transform.localRotation = Quaternion.identity;
            layerRoot.transform.localScale = Vector3.one;

            // 配置Canvas
            Canvas canvas = layerRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UICamera;
            canvas.sortingOrder = (int)layer * 3000;

            // 配置CanvasScaler
            CanvasScaler scaler = layerRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 添加GraphicRaycaster
            layerRoot.AddComponent<GraphicRaycaster>();

            // 存储层级数据
            UILayerData layerData = new UILayerData
            {
                LayerRoot = layerRoot,
                RootCanvas = canvas,
                StartLayerId = (int)layer * 100,
                EndLayerId = (int)layer * 100 + 99
            };
            UILayerData[layer] = layerData;

            XLog.InfoFormat("初始化 UI 层级: {0}, SortingOrder: {1}", layer, canvas.sortingOrder);
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 每帧更新，处理延迟释放逻辑
        /// </summary>
        private void Update()
        {
            ProcessDelayedRelease();
        }

        /// <summary>
        /// 处理延迟释放队列
        /// </summary>
        private void ProcessDelayedRelease()
        {
            float currentTime = Time.time;

            foreach (var data in loadedUIData.Values)
            {
                if (data.WillReleaseUIIs.Count > 0)
                {
                    // 收集需要释放的Handle
                    var toRelease = ListPool<UII>.Get();

                    foreach (var pair in data.WillReleaseUIIs)
                    {
                        if (currentTime >= pair.Value.closeTime)
                        {
                            toRelease.Add(pair.Key);
                        }
                    }

                    // 释放超时的UI
                    foreach (var handle in toRelease)
                    {
                        if (data.WillReleaseUIIs.TryGetValue(handle, out var releaseData))
                        {
                            UICtrlBase uiCtrl = releaseData.ctrlBase;

                            if (uiCtrl != null)
                            {
                                // 释放资源
                                ((IUICtrlBase)uiCtrl).ReleaseAll();

                                // 销毁GameObject
                                GameObject.Destroy(uiCtrl.gameObject);
                                XLog.DebugFormat("延迟释放UI: Handle={0}", handle.Id);
                            }

                            data.WillReleaseUIIs.Remove(handle);
                        }
                    }
                    
                    // 释放临时列表
                    ListPool<UII>.Release(toRelease);
                }
            }
        }

        public bool TryGetUIConfig(AssetI asset, out UIConfigUnManaged config)
        {
            return IConfigDataCenter.I.TryGetConfigBySingleIndex(new UIConfigUnManaged.PrefabAssetIndex(asset),
                out config);
        }

        public async UniTask<UII> OpenWindow(CfgI cfgId)
        {
            // 转换为泛型配置ID
            var id = cfgId.As<UIConfigUnManaged>();

            // 加载UI资源
            if (!loadedUIData.TryGetValueByKey2(id, out LoadedUIData data))
            {
                data = await LoadUI(id);
                if (data == null)
                {
                    XLog.ErrorFormat("加载 UI 失败: {0}", cfgId);
                    return default;
                }
            }

            // 步骤1：检测资源是否合法
            if (data.Status != EAssetStatus.Success)
            {
                XLog.WarningFormat("UI 资源状态异常: {0}, 状态: {1}, 尝试重新加载", cfgId, data.Status);
                // 重新加载
                data = await LoadUI(id);
                if (data == null || data.Status != EAssetStatus.Success)
                {
                    XLog.ErrorFormat("重新加载 UI 失败: {0}, 状态: {1}", cfgId, data?.Status);
                    return default;
                }

                XLog.InfoFormat("重新加载 UI 成功: {0}", cfgId);
            }

            if (!(data.Config is UIWindowConfig windowConfig))
            {
                XLog.ErrorFormat("UI 配置类型错误: {0}", cfgId);
                return default;
            }

            // 步骤2：判断对象池中是否有可用实例
            GameObject uiInstance = null;
            if (data.WillReleaseUIIs.Count > 0)
            {
                var firstPair = data.WillReleaseUIIs.First();
                UICtrlBase uiCtrl = firstPair.Value.ctrlBase;
                if (uiCtrl != null)
                {
                    uiInstance = uiCtrl.gameObject;
                }

                data.WillReleaseUIIs.Remove(firstPair.Key);

                // 重置UI状态
                if (uiInstance != null)
                {
                    uiInstance.SetActive(false);
                    ResetUIState(uiInstance);
                    XLog.InfoFormat("从对象池复用 UI: {0}", cfgId);
                }
                else
                {
                    XLog.WarningFormat("对象池中的UI实例为null: {0}", cfgId);
                    uiInstance = null;
                }
            }

            // 步骤3：根据实例类型处理
            UII handle = default;

            switch (windowConfig.InstanceType)
            {
                case EUIInstanceType.Normal:
                    handle = OpenNormalUI(data, windowConfig, ref uiInstance);
                    break;

                case EUIInstanceType.Multiple:
                    handle = OpenMultipleUI(data, windowConfig);
                    break;

                case EUIInstanceType.Standalone:
                    handle = OpenStandaloneUI(data, windowConfig, ref uiInstance);
                    break;

                case EUIInstanceType.StackStandalone:
                    handle = OpenStackStandaloneUI(data, windowConfig, ref uiInstance);
                    break;
            }

            if (handle.Equals(default(UII)))
            {
                XLog.ErrorFormat("打开 UI 失败: {0}", cfgId);
                return default;
            }

            // 步骤4：处理全屏遮挡逻辑
            if (windowConfig.IsShowMask)
            {
                HandleFullScreenMask(windowConfig.Layer);
            }

            // 步骤5：激活UI并调用生命周期
            if (data.ActiveUIIs.TryGetValue(handle, out UICtrlBase activeCtrl))
            {
                activeCtrl.gameObject.SetActive(true);

                // 调用UI生命周期OnShow
                if (activeCtrl != null)
                {
                    activeCtrl.OnShow();

                    // 递归显示子UI
                    ShowSubUIs(activeCtrl);

                    XLog.DebugFormat("调用 UI OnShow: {0}", cfgId);
                }

                XLog.InfoFormat("成功打开 UI: {0}", cfgId);
            }

            return handle;
        }

        /// <summary>
        /// 打开普通实例UI
        /// </summary>
        private UII OpenNormalUI(LoadedUIData data, UIWindowConfig config, ref GameObject uiInstance)
        {
            // 如果对象池没有，实例化新对象
            if (uiInstance == null)
            {
                uiInstance = GameObject.Instantiate(data.UIPrefabTemplate);
            }

            // 获取UICtrlBase组件
            UICtrlBase uiCtrl = uiInstance.GetComponent<UICtrlBase>();
            if (uiCtrl == null)
            {
                XLog.ErrorFormat("UI实例上未找到UICtrlBase组件: {0}", config.Layer);
                return default;
            }

            // 设置父级到对应 Layer
            uiInstance.transform.SetParent(UILayerData[config.Layer].LayerRoot.transform, false);
            uiInstance.transform.localPosition = Vector3.zero;
            uiInstance.transform.localRotation = Quaternion.identity;
            uiInstance.transform.localScale = Vector3.one;

            // 生成 UII
            int instanceId = GenerateInstanceId();
            UII handle = GenerateUII(config.UITypeI, instanceId);
            uiCtrl.Id = handle;

            // 添加到活跃列表
            data.ActiveUIIs[handle] = uiCtrl;

            XLog.InfoFormat("打开普通 UI: {0}, InstanceId: {1}", config.Layer, instanceId);
            return handle;
        }

        /// <summary>
        /// 打开多实例UI
        /// </summary>
        private UII OpenMultipleUI(LoadedUIData data, UIWindowConfig config)
        {
            // Multiple 总是创建新实例
            GameObject uiInstance =
                GameObject.Instantiate(data.UIPrefabTemplate, UILayerData[config.Layer].LayerRoot.transform, false);
            // 设置父级
            uiInstance.transform.localPosition = Vector3.zero;
            uiInstance.transform.localRotation = Quaternion.identity;
            uiInstance.transform.localScale = Vector3.one;

            // 获取UICtrlBase组件
            UICtrlBase uiCtrl = uiInstance.GetComponent<UICtrlBase>();
            if (uiCtrl == null)
            {
                XLog.ErrorFormat("UI实例上未找到UICtrlBase组件: {0}", config.Layer);
                return default;
            }

            // 生成唯一 UII
            int instanceId = GenerateInstanceId();
            UII handle = GenerateUII(config.UITypeI, instanceId);
            uiCtrl.Id = handle;

            // 添加到活跃列表
            data.ActiveUIIs[handle] = uiCtrl;

            XLog.InfoFormat("打开多实例 UI: {0}, InstanceId: {1}", config.Layer, instanceId);
            return handle;
        }

        /// <summary>
        /// 打开单例UI
        /// </summary>
        private UII OpenStandaloneUI(LoadedUIData data, UIWindowConfig config, ref GameObject uiInstance)
        {
            // 检查是否已有实例
            if (data.ActiveUIIs.Count > 0)
            {
                // 已有实例，增加栈帧引用
                var existingPair = data.ActiveUIIs.First();
                UII existingHandle = existingPair.Key;
                UICtrlBase existingCtrl = existingPair.Value;

                // 将实例显示到顶部
                existingCtrl.transform.SetAsLastSibling();

                // 添加到独立栈
                AddUIToStandaloneStack(config.Layer, existingHandle);

                XLog.InfoFormat("复用单例 UI: {0}", config.Layer);
                return existingHandle;
            }
            else
            {
                // 无实例，创建新实例
                if (uiInstance == null)
                {
                    uiInstance = GameObject.Instantiate(data.UIPrefabTemplate);
                }

                // 获取UICtrlBase组件
                UICtrlBase uiCtrl = uiInstance.GetComponent<UICtrlBase>();
                if (uiCtrl == null)
                {
                    XLog.ErrorFormat("UI实例上未找到UICtrlBase组件: {0}", config.Layer);
                    return default;
                }

                // 设置父级
                uiInstance.transform.SetParent(UILayerData[config.Layer].LayerRoot.transform, false);
                uiInstance.transform.localPosition = Vector3.zero;
                uiInstance.transform.localRotation = Quaternion.identity;
                uiInstance.transform.localScale = Vector3.one;

                // 生成 UII
                UII handle = GenerateUII(config.UITypeI, 0);
                uiCtrl.Id = handle;

                // 添加到活跃列表
                data.ActiveUIIs[handle] = uiCtrl;

                // 添加到独立栈
                AddUIToStandaloneStack(config.Layer, handle);

                XLog.InfoFormat("创建单例 UI: {0}", config.Layer);
                return handle;
            }
        }

        /// <summary>
        /// 打开栈单例UI
        /// </summary>
        private UII OpenStackStandaloneUI(LoadedUIData data, UIWindowConfig config, ref GameObject uiInstance)
        {
            // 检查栈中是否已有该 UI
            UII? existingHandle = FindUIInStack(config.Layer, config.UITypeI);

            if (existingHandle.HasValue)
            {
                // 栈中已有，移到栈顶
                UICtrlBase existingCtrl = data.ActiveUIIs[existingHandle.Value];

                // 从当前栈位置移除
                RemoveUIFromStack(config.Layer, existingHandle.Value);

                // 添加到栈顶
                AddUIToStack(config.Layer, existingHandle.Value);

                // 显示到顶部
                existingCtrl.transform.SetAsLastSibling();

                XLog.InfoFormat("栈单例 UI 移至栈顶: {0}", config.Layer);
                return existingHandle.Value;
            }
            else
            {
                // 栈中没有，创建新实例
                if (uiInstance == null)
                {
                    uiInstance = GameObject.Instantiate(data.UIPrefabTemplate);
                }

                // 获取UICtrlBase组件
                UICtrlBase uiCtrl = uiInstance.GetComponent<UICtrlBase>();
                if (uiCtrl == null)
                {
                    XLog.ErrorFormat("UI实例上未找到UICtrlBase组件: {0}", config.Layer);
                    return default;
                }

                // 设置父级
                uiInstance.transform.SetParent(UILayerData[config.Layer].LayerRoot.transform, false);
                uiInstance.transform.localPosition = Vector3.zero;
                uiInstance.transform.localRotation = Quaternion.identity;
                uiInstance.transform.localScale = Vector3.one;

                // 生成 UII
                int instanceId = GenerateInstanceId();
                UII handle = GenerateUII(config.UITypeI, instanceId);
                uiCtrl.Id = handle;

                // 添加到活跃列表
                data.ActiveUIIs[handle] = uiCtrl;

                // 添加到栈顶
                AddUIToStack(config.Layer, handle);

                XLog.InfoFormat("创建栈单例 UI: {0}, InstanceId: {1}", config.Layer, instanceId);
                return handle;
            }
        }

        /// <summary>
        /// 处理全屏遮挡逻辑
        /// </summary>
        private void HandleFullScreenMask(EUILayer layer)
        {
            // 遍历该 Layer 的栈，隐藏下层UI
            var layerData = UILayerData[layer];
            var stackNode = layerData.UIStacks.Last;

            // 从栈顶往下遍历（跳过当前UI，即最后一个）
            if (stackNode != null && stackNode.Previous != null)
            {
                stackNode = stackNode.Previous;

                while (stackNode != null)
                {
                    stackNode.Value.IsHideByStack = true;
                    // 同步到GameObject
                    SetUIActiveByHandle(stackNode.Value.Handle, false);
                    stackNode = stackNode.Previous;
                }
            }

            // 遍历更低层级的 Layer，也隐藏
            for (int i = (int)layer - 1; i > 0; i--)
            {
                EUILayer lowerLayer = (EUILayer)i;
                if (UILayerData.TryGetValue(lowerLayer, out var lowerLayerData))
                {
                    HideLayerByStack(lowerLayer);
                }
            }

            XLog.InfoFormat("全屏遮挡处理完成: {0}", layer);
        }

        /// <summary>
        /// 隐藏整个Layer
        /// </summary>
        private void HideLayerByStack(EUILayer layer)
        {
            var layerData = UILayerData[layer];
            var node = layerData.UIStacks.First;
            while (node != null)
            {
                node.Value.IsHideByStack = true;
                // 同步到GameObject
                SetUIActiveByHandle(node.Value.Handle, false);
                node = node.Next;
            }
        }

        /// <summary>
        /// 根据Handle设置UI的Active状态
        /// </summary>
        private void SetUIActiveByHandle(UII handle, bool active)
        {
            // 通过config索引直接查找，避免遍历所有数据
            var configId = handle.TypeI.As<UIConfigUnManaged>();
            if (loadedUIData.TryGetValueByKey2(configId, out LoadedUIData data))
            {
                if (data.ActiveUIIs.TryGetValue(handle, out UICtrlBase uiCtrl))
                {
                    if (uiCtrl != null)
                    {
                        uiCtrl.gameObject.SetActive(active);
                        XLog.DebugFormat("设置 UI Active状态: Handle={0}, Active={1}", handle.Id, active);
                    }
                    else
                    {
                        XLog.WarningFormat("UI控制器为null，无法设置Active状态: Handle={0}", handle.Id);
                    }

                    return;
                }
            }

            XLog.WarningFormat("未找到UI实例，无法设置Active状态: Handle={0}", handle.Id);
        }

        /// <summary>
        /// 重置UI状态（从对象池复用时使用）
        /// </summary>
        private void ResetUIState(GameObject uiInstance)
        {
            if (uiInstance == null) return;

            // 重置Transform
            uiInstance.transform.localPosition = Vector3.zero;
            uiInstance.transform.localRotation = Quaternion.identity;
            uiInstance.transform.localScale = Vector3.one;

            // 重置Canvas相关组件
            Canvas canvas = uiInstance.GetComponent<Canvas>();
            if (canvas != null && canvas.overrideSorting)
            {
                canvas.sortingOrder = 0;
            }

            XLog.DebugFormat("重置 UI 状态: {0}", uiInstance.name);
        }

        /// <summary>
        /// 递归显示子UI
        /// </summary>
        private void ShowSubUIs(IUICtrlBase parentCtrl)
        {
            if (parentCtrl == null) return;

            if (parentCtrl.StaticSubUICtrl != null && parentCtrl.StaticSubUICtrl.Count > 0)
            {
                foreach (var subUI in parentCtrl.StaticSubUICtrl)
                {
                    if (subUI != null)
                    {
                        subUI.OnShow();
                        ShowSubUIs(subUI);
                    }
                }
            }
        }

        /// <summary>
        /// 添加UI到独立栈（用于Standalone类型）
        /// </summary>
        private void AddUIToStandaloneStack(EUILayer layer, UII handle)
        {
            var layerData = UILayerData[layer];

            // 计算Canvas层级ID（独立栈不受普通栈影响）
            int canvasLayerId = layerData.StartLayerId + 50; // 独立栈从中间位置开始

            var uiStack = new UIStack
            {
                Handle = handle,
                IsHideSelf = false,
                IsHideByStack = false,
                CanvasLayerId = canvasLayerId
            };

            // 检查是否已存在
            bool exists = false;
            foreach (var stack in layerData.StandAloneStacks)
            {
                if (stack.Handle.Equals(handle))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                layerData.StandAloneStacks.Add(uiStack);

                // 更新UI的Canvas sortingOrder
                UpdateUICanvasOrder(handle, canvasLayerId);

                XLog.DebugFormat("UI 添加到独立栈: Layer={0}, Handle={1}, CanvasLayerId={2}",
                    layer, handle.Id, canvasLayerId);
            }
        }

        /// <summary>
        /// 从独立栈中移除UI
        /// </summary>
        private void RemoveUIFromStandaloneStack(EUILayer layer, UII handle)
        {
            var layerData = UILayerData[layer];

            for (int i = layerData.StandAloneStacks.Count - 1; i >= 0; i--)
            {
                if (layerData.StandAloneStacks[i].Handle.Equals(handle))
                {
                    layerData.StandAloneStacks.RemoveAt(i);
                    XLog.DebugFormat("UI 从独立栈中移除: Layer={0}, Handle={1}", layer, handle.Id);
                    break;
                }
            }
        }

        /// <summary>
        /// 递归关闭子UI
        /// </summary>
        private void CloseSubUIs(UICtrlBase parentCtrl)
        {
            if (parentCtrl == null) return;

            if (parentCtrl.StaticSubUICtrl is { Count: > 0 })
            {
                foreach (var subUI in parentCtrl.StaticSubUICtrl)
                {
                    if (subUI != null)
                    {
                        subUI.OnClose();
                        CloseSubUIs(subUI as UICtrlBase);
                        subUI.ReleaseAll();
                        XLog.DebugFormat("关闭子 UI");
                    }
                }
            }

            if (parentCtrl.DynamicLoadBox is { Count: > 0 })
            {
                foreach (var subUI in parentCtrl.DynamicLoadBox)
                {
                    if (subUI != null)
                    {
                        // TODO 这里需要调用LoadBox释放
                    }
                }
            }
        }

        /// <summary>
        /// 根据实例类型处理关闭逻辑
        /// </summary>
        private void CloseUIByInstanceType(LoadedUIData data, UIWindowConfig config, UII handle,
            UICtrlBase uiCtrl)
        {
            switch (config.InstanceType)
            {
                case EUIInstanceType.Normal:
                    // 普通实例：从栈中移除
                    RemoveUIFromStack(config.Layer, handle);
                    break;

                case EUIInstanceType.Multiple:
                    // 多实例：从栈中移除，统一进入对象池
                    RemoveUIFromStack(config.Layer, handle);
                    break;

                case EUIInstanceType.Standalone:
                    // 单例：从独立栈中移除
                    RemoveUIFromStandaloneStack(config.Layer, handle);
                    break;

                case EUIInstanceType.StackStandalone:
                    // 栈单例：从栈中移除
                    RemoveUIFromStack(config.Layer, handle);
                    break;
            }

            // 隐藏UI
            uiCtrl.gameObject.SetActive(false);
        }

        /// <summary>
        /// 移动UI到None层级并加入延迟释放队列
        /// </summary>
        private void MoveUIToNoneLayer(UICtrlBase uiInstance, LoadedUIData data, UII handle)
        {
            if (uiInstance == null)
            {
                XLog.WarningFormat("UI实例为null，无法移动到None层级: Handle={0}", handle.Id);
                return;
            }

            if (!UILayerData.TryGetValue(EUILayer.None, out var noneLayerData))
            {
                XLog.ErrorFormat("未找到None层级数据");
                return;
            }

            // 移动到None层级
            uiInstance.transform.SetParent(noneLayerData.LayerRoot.transform, false);
            uiInstance.gameObject.SetActive(false);

            // 加入延迟释放队列（默认延迟5秒）
            float delay = 5.0f;
            data.WillReleaseUIIs[handle] = (uiInstance, Time.time + delay);

            XLog.DebugFormat("UI 移动到None层级并加入延迟释放队列: Handle={0}, Delay={1}s", handle.Id, delay);
        }

        /// <summary>
        /// 恢复被栈隐藏的窗口
        /// </summary>
        private void RestoreHiddenUIs(EUILayer layer)
        {
            // 恢复当前Layer的被隐藏UI
            var layerData = UILayerData[layer];
            var node = layerData.UIStacks.Last;

            // 找到新的栈顶UI（如果有）
            if (node != null)
            {
                // 检查新的栈顶UI是否也是全屏遮挡
                bool newTopIsFullScreen = false;

                // 从栈顶往下检查，找到第一个IsShowMask的UI
                var checkNode = node;
                while (checkNode != null)
                {
                    if (!checkNode.Value.IsHideSelf)
                    {
                        // 通过config索引直接查找，避免遍历所有数据
                        var configId = checkNode.Value.Handle.TypeI.As<UIConfigUnManaged>();
                        if (loadedUIData.TryGetValueByKey2(configId, out LoadedUIData data))
                        {
                            if (data.Config is UIWindowConfig config &&
                                data.ActiveUIIs.ContainsKey(checkNode.Value.Handle))
                            {
                                if (config.IsShowMask)
                                {
                                    newTopIsFullScreen = true;
                                }
                            }
                        }

                        break;
                    }

                    checkNode = checkNode.Previous;
                }

                // 如果新的栈顶不是全屏遮挡，恢复之前被隐藏的UI
                if (!newTopIsFullScreen)
                {
                    RestoreLayerUIs(layer);

                    // 恢复更低层级的UI
                    for (int i = (int)layer - 1; i > 0; i--)
                    {
                        EUILayer lowerLayer = (EUILayer)i;
                        if (UILayerData.TryGetValue(lowerLayer, out var lowerLayerData))
                        {
                            RestoreLayerUIs(lowerLayer);
                        }
                    }
                }
            }
            else
            {
                // 栈空了，恢复所有被隐藏的UI
                RestoreLayerUIs(layer);

                // 恢复更低层级的UI
                for (int i = (int)layer - 1; i > 0; i--)
                {
                    EUILayer lowerLayer = (EUILayer)i;
                    if (UILayerData.TryGetValue(lowerLayer, out var lowerLayerData))
                    {
                        RestoreLayerUIs(lowerLayer);
                    }
                }
            }

            XLog.DebugFormat("恢复被栈隐藏的UI: Layer={0}", layer);
        }

        /// <summary>
        /// 恢复指定Layer的所有被隐藏UI
        /// </summary>
        private void RestoreLayerUIs(EUILayer layer)
        {
            var layerData = UILayerData[layer];
            var node = layerData.UIStacks.First;

            while (node != null)
            {
                if (node.Value.IsHideByStack && !node.Value.IsHideSelf)
                {
                    node.Value.IsHideByStack = false;
                    // 同步到GameObject
                    SetUIActiveByHandle(node.Value.Handle, true);
                }

                node = node.Next;
            }
        }

        /// <summary>
        /// 生成全局唯一的实例ID
        /// </summary>
        private int GenerateInstanceId()
        {
            return ++_globalInstanceIdCounter;
        }

        /// <summary>
        /// 生成UII
        /// </summary>
        private UII GenerateUII(CfgI typeId, int instanceId)
        {
            return new UII
            {
                TypeI = typeId,
                IsWidget = 0,
                Id = instanceId
            };
        }

        /// <summary>
        /// 添加UI到栈
        /// </summary>
        private void AddUIToStack(EUILayer layer, UII handle)
        {
            if (!UILayerData.TryGetValue(layer, out var layerData))
            {
                XLog.ErrorFormat("未找到UI层级数据: {0}", layer);
                return;
            }

            // 计算Canvas层级ID
            int stackPosition = layerData.UIStacks.Count;
            int canvasLayerId = layerData.StartLayerId + stackPosition;

            // 确保不超过层级范围
            if (canvasLayerId > layerData.EndLayerId)
            {
                XLog.WarningFormat("UI栈已满，无法添加更多UI到层级: {0}, 当前数量: {1}", layer, stackPosition);
                canvasLayerId = layerData.EndLayerId;
            }

            var uiStack = new UIStack
            {
                Handle = handle,
                IsHideSelf = false,
                IsHideByStack = false,
                CanvasLayerId = canvasLayerId
            };

            layerData.UIStacks.AddLast(uiStack);

            // 更新UI的Canvas sortingOrder
            UpdateUICanvasOrder(handle, canvasLayerId);

            XLog.DebugFormat("UI 添加到栈: Layer={0}, Handle={1}, CanvasLayerId={2}", layer, handle.Id, canvasLayerId);
        }

        /// <summary>
        /// 更新UI的Canvas sortingOrder
        /// </summary>
        private void UpdateUICanvasOrder(UII handle, int canvasLayerId)
        {
            // 通过config索引直接查找，避免遍历所有数据
            var configId = handle.TypeI.As<UIConfigUnManaged>();
            if (loadedUIData.TryGetValueByKey2(configId, out LoadedUIData data))
            {
                if (data.ActiveUIIs.TryGetValue(handle, out UICtrlBase uiCtrl))
                {
                    if (uiCtrl == null)
                    {
                        XLog.WarningFormat("UI控制器为null，无法更新Canvas: Handle={0}", handle.Id);
                        return;
                    }

                    GameObject uiInstance = uiCtrl.gameObject;
                    Canvas canvas = uiInstance.GetComponent<Canvas>();
                    if (canvas == null)
                    {
                        canvas = uiInstance.AddComponent<Canvas>();
                        canvas.overrideSorting = true;
                    }

                    canvas.sortingOrder = canvasLayerId;
                    XLog.DebugFormat("更新 UI Canvas sortingOrder: Handle={0}, Order={1}", handle.Id, canvasLayerId);
                    return;
                }
            }

            XLog.WarningFormat("未找到UI实例，无法更新Canvas: Handle={0}", handle.Id);
        }

        /// <summary>
        /// 从栈中移除UI
        /// </summary>
        private void RemoveUIFromStack(EUILayer layer, UII handle)
        {
            if (!UILayerData.TryGetValue(layer, out var layerData))
            {
                XLog.ErrorFormat("未找到UI层级数据: {0}", layer);
                return;
            }

            var node = layerData.UIStacks.First;

            while (node != null)
            {
                if (node.Value.Handle.Equals(handle))
                {
                    layerData.UIStacks.Remove(node);
                    XLog.DebugFormat("UI 从栈中移除: Layer={0}, Handle={1}", layer, handle.Id);
                    return;
                }

                node = node.Next;
            }

            XLog.WarningFormat("未在栈中找到UI: Layer={0}, Handle={1}", layer, handle.Id);
        }

        /// <summary>
        /// 在栈中查找UI
        /// </summary>
        private UII? FindUIInStack(EUILayer layer, CfgI uiTypeI)
        {
            var layerData = UILayerData[layer];
            var node = layerData.UIStacks.First;

            while (node != null)
            {
                if (node.Value.Handle.TypeI.Equals(uiTypeI))
                {
                    return node.Value.Handle;
                }

                node = node.Next;
            }

            return null;
        }

        /// <summary>
        /// 根据UII查找对应的LoadedUIData
        /// </summary>
        private bool TryFindLoadedUIData(UII handle, out LoadedUIData data, out UIWindowConfig config)
        {
            data = null;
            config = null;

            // 通过config索引直接查找，避免遍历所有数据
            var configId = handle.TypeI.As<UIConfigUnManaged>();
            if (loadedUIData.TryGetValueByKey2(configId, out data))
            {
                if (data.ActiveUIIs.ContainsKey(handle))
                {
                    config = data.Config as UIWindowConfig;
                    return config != null;
                }
            }

            return false;
        }


        public void CloseUI(UII uiHandle)
        {
            // 步骤1：查找UI数据
            if (!TryFindLoadedUIData(uiHandle, out LoadedUIData data, out UIWindowConfig config))
            {
                XLog.WarningFormat("未找到UI数据: Handle={0}", uiHandle.Id);
                return;
            }

            // 步骤2：验证Handle是否存在
            if (!data.ActiveUIIs.TryGetValue(uiHandle, out UICtrlBase uiInstance))
            {
                XLog.WarningFormat("未找到指定Handle的UI实例: Handle={0}", uiHandle.Id);
                return;
            }

            // 步骤3：调用UI生命周期OnClose
            if (uiInstance != null)
            {
                uiInstance.OnClose();

                // 递归关闭子UI
                CloseSubUIs(uiInstance);

                XLog.DebugFormat("调用 UI OnClose: Handle={0}", uiHandle.Id);
            }

            // 步骤4：根据实例类型处理关闭逻辑
            CloseUIByInstanceType(data, config, uiHandle, uiInstance);

            // 步骤5：从ActiveUIIs移除
            data.ActiveUIIs.Remove(uiHandle);

            // 步骤6：移动到None层级并延迟释放
            MoveUIToNoneLayer(uiInstance, data, uiHandle);

            // 步骤7：恢复被栈隐藏的窗口
            if (config.IsShowMask)
            {
                RestoreHiddenUIs(config.Layer);
            }

            XLog.InfoFormat("成功关闭 UI: Handle={0}", uiHandle.Id);
        }

        public void HideUI(UII uiHandle)
        {
            // 步骤1：查找UI数据
            if (!TryFindLoadedUIData(uiHandle, out LoadedUIData data, out UIWindowConfig config))
            {
                XLog.WarningFormat("未找到UI数据: Handle={0}", uiHandle.Id);
                return;
            }

            // 步骤2：验证Handle是否存在
            if (!data.ActiveUIIs.TryGetValue(uiHandle, out UICtrlBase uiCtrl))
            {
                XLog.WarningFormat("未找到指定Handle的UI实例: Handle={0}", uiHandle.Id);
                return;
            }

            // 步骤3：隐藏UI
            uiCtrl.gameObject.SetActive(false);

            // 步骤4：调用UI生命周期OnHide
            if (uiCtrl != null)
            {
                uiCtrl.OnHide();

                // 递归隐藏子UI
                HideSubUIs(uiCtrl);

                XLog.DebugFormat("调用 UI OnHide: Handle={0}", uiHandle.Id);
            }

            // 步骤5：更新栈状态
            var layerData = UILayerData[config.Layer];
            var stackNode = layerData.UIStacks.First;
            while (stackNode != null)
            {
                if (stackNode.Value.Handle.Equals(uiHandle))
                {
                    stackNode.Value.IsHideSelf = true;
                    break;
                }

                stackNode = stackNode.Next;
            }

            XLog.InfoFormat("成功隐藏 UI: Handle={0}", uiHandle.Id);
        }

        public void ShowUI(UII uiHandle)
        {
            // 步骤1：查找UI数据
            if (!TryFindLoadedUIData(uiHandle, out LoadedUIData data, out UIWindowConfig config))
            {
                XLog.WarningFormat("未找到UI数据: Handle={0}", uiHandle.Id);
                return;
            }

            // 步骤2：验证Handle是否存在
            if (!data.ActiveUIIs.TryGetValue(uiHandle, out UICtrlBase uiCtrl))
            {
                XLog.WarningFormat("未找到指定Handle的UI实例: Handle={0}", uiHandle.Id);
                return;
            }

            // 步骤3：显示UI
            uiCtrl.gameObject.SetActive(true);

            // 步骤4：调用UI生命周期OnShow
            if (uiCtrl != null)
            {
                uiCtrl.OnShow();

                // 递归显示子UI
                ShowSubUIs(uiCtrl);

                XLog.DebugFormat("调用 UI OnShow: Handle={0}", uiHandle.Id);
            }

            // 步骤5：更新栈状态
            var layerData = UILayerData[config.Layer];
            var stackNode = layerData.UIStacks.First;
            while (stackNode != null)
            {
                if (stackNode.Value.Handle.Equals(uiHandle))
                {
                    stackNode.Value.IsHideSelf = false;
                    break;
                }

                stackNode = stackNode.Next;
            }

            XLog.InfoFormat("成功显示 UI: Handle={0}", uiHandle.Id);
        }

        /// <summary>
        /// 递归隐藏子UI
        /// </summary>
        private void HideSubUIs(UICtrlBase parentCtrl)
        {
            if (parentCtrl == null) return;

            if (parentCtrl.StaticSubUICtrl != null && parentCtrl.StaticSubUICtrl.Count > 0)
            {
                foreach (var subUI in parentCtrl.StaticSubUICtrl)
                {
                    if (subUI != null)
                    {
                        subUI.OnHide();
                        // 递归隐藏子UI的子UI
                        HideSubUIs(subUI as UICtrlBase);
                        XLog.DebugFormat("隐藏子 UI");
                    }
                }
            }

            if (parentCtrl.DynamicLoadBox is { Count: > 0 })
            {
                foreach (var subUI in parentCtrl.DynamicLoadBox)
                {
                    if (subUI != null)
                    {
                        // 调用loadbox隐藏
                    }
                }
            }
        }

        /// <summary>
        /// 注册UI类型
        /// </summary>
        /// <param name="uiConfigId">UI配置ID</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterUI(CfgI uiConfigId)
        {
            // 转换为泛型配置ID
            var cfgId = uiConfigId.As<UIConfigUnManaged>();

            // 检查是否已经注册
            if (loadedUIData.ContainsKey2(cfgId))
            {
                XLog.WarningFormat("UI已注册: {0}", uiConfigId);
                return false;
            }

            // 预加载UI资源（可选，也可以延迟加载）
            // 这里只做注册标记，实际加载在OpenUI时进行
            XLog.InfoFormat("注册UI: ConfigId: {0}", uiConfigId);
            return true;
        }

        public void ReleaseUICtrl(IUICtrlBase uiCtrl)
        {
            if (uiCtrl == null)
            {
                XLog.Warning("尝试释放空的UI控制器");
                return;
            }

            // 调用OnClose生命周期
            uiCtrl.OnClose();

            // 递归关闭子UI
            CloseSubUIs(uiCtrl as UICtrlBase);

            // 释放资源
            uiCtrl.ReleaseAll();

            // 查找并移除对应的UI实例
            foreach (var data in loadedUIData.Values)
            {
                var handlesToRemove = ListPool<UII>.Get();
                foreach (var kvp in data.ActiveUIIs)
                {
                    UICtrlBase ctrl = kvp.Value;
                    if (ctrl != null && ReferenceEquals(ctrl, uiCtrl))
                    {
                        handlesToRemove.Add(kvp.Key);
                    }
                }

                foreach (var handle in handlesToRemove)
                {
                    if (data.ActiveUIIs.TryGetValue(handle, out UICtrlBase uiInstance))
                    {
                        // 从栈中移除
                        if (data.Config is UIWindowConfig config)
                        {
                            RemoveUIFromStack(config.Layer, handle);
                        }

                        // 销毁GameObject
                        GameObject.Destroy(uiInstance.gameObject);
                        data.ActiveUIIs.Remove(handle);
                        XLog.InfoFormat("释放UI控制器: Handle={0}", handle.Id);
                    }
                }
                
                // 释放临时列表
                ListPool<UII>.Release(handlesToRemove);
            }
        }

        /// <summary>
        /// 创建UI控制器实例（公共方法）
        /// </summary>
        /// <param name="prefab">UI预制体</param>
        /// <param name="count">创建数量</param>
        /// <param name="uiHandles">输出的UI句柄列表</param>
        /// <param name="typeId">UI类型ID（可选，用于配置创建的UI）</param>
        /// <param name="assetHandle">资源句柄（可选，用于保存到控制器）</param>
        /// <param name="createdInstances">输出的创建的UICtrlBase实例列表（可选）</param>
        /// <returns>是否创建成功</returns>
        private bool CreateUICtrlInstances(
            GameObject prefab, 
            int count,
            CfgI typeId = default, 
            XAssetHandle assetHandle = null,
            List<UICtrlBase> createdInstances = null)
        {
            if (prefab == null)
            {
                XLog.Error("预制体为空");
                return false;
            }

            bool hasTypeI = !typeId.Equals(default(CfgI));
            bool success = false;

            for (int i = 0; i < count; i++)
            {
                var instance = GameObject.Instantiate(prefab);
                var ctrl = instance.GetComponent<UICtrlBase>();
                if (ctrl == null)
                {
                    GameObject.Destroy(instance);
                    XLog.WarningFormat("预制体上未找到UICtrlBase组件");
                    continue;
                }

                // 生成UII
                UII uiHandle;
                if (hasTypeI)
                {
                    uiHandle = GenerateUII(typeId, i);
                }
                else
                {
                    uiHandle = new UII
                    {
                        TypeI = default,
                        Id = i
                    };
                }

                uiHandle.IsWidget = 1; // 标记为Widget
                ctrl.Id = uiHandle;

                // 保存资源句柄到控制器（只在第一个实例时保存，多个实例共享同一资源）
                if (assetHandle != null && i == 0)
                {
                    if (ctrl.LoadedAssetIdList == null)
                    {
                        ctrl.LoadedAssetIdList = ListPool<XAssetHandle>.Get();
                    }

                    ctrl.LoadedAssetIdList.Add(assetHandle);
                }

                createdInstances?.Add(ctrl);
                success = true;

                XLog.DebugFormat("创建UI控制器: Handle={0}, Index={1}", uiHandle.Id, i);
            }

            return success;
        }

        public async UniTask CreateUICtrlByAssetId(ModI mod, string path, int count, List<UII> uiHandles)
        {
            if (uiHandles == null)
            {
                XLog.Error("uiHandles列表为空");
                return;
            }

            uiHandles.Clear();

            // 创建资源ID
            var assetId = IAssetManager.I.CreateAssetId<AssetI>(mod, path);
            if (!assetId.Valid)
            {
                XLog.ErrorFormat("创建资源ID失败: Mod={0}, Path={1}", mod, path);
                return;
            }

            // 加载资源
            var handle = await assetId.CreateHandleAsync();
            if (handle == null)
            {
                XLog.ErrorFormat("加载资源失败: AssetId={0}", assetId);
                return;
            }

            var prefab = handle.Get<GameObject>();
            if (prefab == null)
            {
                handle.Release();
                XLog.ErrorFormat("获取预制体失败: AssetId={0}", assetId);
                return;
            }

            // 创建UI控制器实例
            CreateUICtrlInstances(prefab, count, default, handle);

            // 注意：这里不释放资源句柄，因为UI可能还在使用
            // 资源释放应该在UI关闭时进行
        }

        /// <summary>
        /// 通过配置ID创建UI控制器
        /// </summary>
        /// <param name="id">UI配置ID</param>
        /// <param name="count">创建数量</param>
        /// <param name="uiHandles">输出的UI句柄列表</param>
        /// <returns>异步任务</returns>
        public async UniTask CreateUICtrlByConfig(CfgI id, int count, List<UII> uiHandles)
        {
            if (uiHandles == null)
            {
                XLog.Error("uiHandles列表为空");
                return;
            }

            uiHandles.Clear();

            // 转换为泛型配置ID
            var cfgId = id.As<UIConfigUnManaged>();

            // 加载UI资源
            LoadedUIData data;
            if (!loadedUIData.TryGetValueByKey2(cfgId, out data))
            {
                data = await LoadUI(cfgId);
                if (data == null)
                {
                    XLog.ErrorFormat("加载 UI 失败: {0}", id);
                    return;
                }
            }

            // 检测资源是否合法
            if (data.Status != EAssetStatus.Success)
            {
                XLog.WarningFormat("UI 资源状态异常: {0}, 状态: {1}, 尝试重新加载", id, data.Status);
                // 重新加载
                data = await LoadUI(cfgId);
                if (data == null || data.Status != EAssetStatus.Success)
                {
                    XLog.ErrorFormat("重新加载 UI 失败: {0}, 状态: {1}", id, data?.Status);
                    return;
                }

                XLog.InfoFormat("重新加载 UI 成功: {0}", id);
            }

            if (data.UIPrefabTemplate == null)
            {
                XLog.ErrorFormat("UI预制体模板为空: {0}", id);
                return;
            }

            // 获取配置信息
            if (!(data.Config is UIWindowConfig windowConfig))
            {
                XLog.ErrorFormat("UI 配置类型错误: {0}", id);
                return;
            }

            // 创建UI控制器实例
            var createdInstances = ListPool<UICtrlBase>.Get();
            if (!CreateUICtrlInstances(data.UIPrefabTemplate, count, windowConfig.UITypeI, null,
                    createdInstances))
            {
                XLog.ErrorFormat("创建UI控制器失败: ConfigId={0}", id);
                ListPool<UICtrlBase>.Release(createdInstances);
                return;
            }

            XLog.InfoFormat("通过配置创建UI控制器完成: ConfigId={0}, Count={1}", id, uiHandles.Count);
            
            // 释放临时列表
            ListPool<UICtrlBase>.Release(createdInstances);
        }

        /// <summary>
        /// 通过资源ID创建UI控制器并返回实例列表（内部方法）
        /// </summary>
        /// <param name="assetId">资源ID</param>
        /// <param name="count">创建数量</param>
        /// <param name="uiHandles">输出的UI句柄列表</param>
        /// <param name="createdInstances">输出的创建的UICtrlBase实例列表</param>
        /// <returns>异步任务</returns>
        internal async UniTask CreateUICtrlByConfigWithInstances(AssetI assetId, int count,
            List<UICtrlBase> createdInstances)
        {
            if (createdInstances == null)
            {
                XLog.Error("输出列表为空");
                return;
            }

            createdInstances.Clear();

            if (!assetId.Valid)
            {
                XLog.ErrorFormat("资源ID无效: AssetId={0}", assetId);
                return;
            }

            if (count <= 0)
            {
                XLog.WarningFormat("创建数量无效: Count={0}", count);
                return;
            }

            // 加载资源
            var handle = await assetId.CreateHandleAsync();
            if (handle == null)
            {
                XLog.ErrorFormat("加载资源失败: AssetId={0}", assetId);
                return;
            }

            var prefab = handle.Get<GameObject>();
            if (prefab == null)
            {
                handle.Release();
                XLog.ErrorFormat("获取预制体失败: AssetId={0}", assetId);
                return;
            }

            // 创建UI控制器实例
            if (!CreateUICtrlInstances(prefab, count, default, handle, createdInstances))
            {
                XLog.ErrorFormat("创建UI控制器失败: AssetId={0}", assetId);
                return;
            }

            XLog.InfoFormat("通过资源ID创建UI控制器完成: AssetId={0}, Count={1}", assetId, createdInstances.Count);
        }

        /// <summary>
        /// 通过资源ID创建UI控制器
        /// </summary>
        /// <param name="assetId">资源ID</param>
        /// <param name="count">创建数量</param>
        /// <param name="uiHandles">输出的UI句柄列表</param>
        /// <returns>异步任务</returns>
        public async UniTask CreateUICtrlByConfig(AssetI assetId, int count, List<UII> uiHandles)
        {
            if (uiHandles == null)
            {
                XLog.Error("uiHandles列表为空");
                return;
            }

            uiHandles.Clear();

            if (!assetId.Valid)
            {
                XLog.ErrorFormat("资源ID无效: AssetId={0}", assetId);
                return;
            }

            if (count <= 0)
            {
                XLog.WarningFormat("创建数量无效: Count={0}", count);
                return;
            }

            // 加载资源
            var handle = await assetId.CreateHandleAsync();
            if (handle == null)
            {
                XLog.ErrorFormat("加载资源失败: AssetId={0}", assetId);
                return;
            }

            var prefab = handle.Get<GameObject>();
            if (prefab == null)
            {
                handle.Release();
                XLog.ErrorFormat("获取预制体失败: AssetId={0}", assetId);
                return;
            }

            // 创建UI控制器实例
            var createdInstances = ListPool<UICtrlBase>.Get();
            if (!CreateUICtrlInstances(prefab, count, default, handle, createdInstances))
            {
                XLog.ErrorFormat("创建UI控制器失败: AssetId={0}", assetId);
                ListPool<UICtrlBase>.Release(createdInstances);
                return;
            }

            XLog.InfoFormat("通过资源ID创建UI控制器完成: AssetId={0}, Count={1}", assetId, uiHandles.Count);
            
            // 释放临时列表
            ListPool<UICtrlBase>.Release(createdInstances);
        }

        public IUICtrlBase GetUICtrlByHandle(UII handle)
        {
            throw new NotImplementedException();
        }


        private async UniTask<LoadedUIData> LoadUI(CfgI<UIConfigUnManaged> cfgId)
        {
            if (!cfgId.TryGetData(out var config))
            {
                XLog.ErrorFormat("获取UI配置数据失败: {0}", cfgId);
                return null;
            }

            // 检查是否已经加载
            var existingData = loadedUIData.GetByKey2(cfgId);
            if (existingData != null)
            {
                XLog.DebugFormat("UI已加载，返回现有数据: {0}", cfgId);
                return existingData;
            }

            var loadUI = new LoadedUIData
            {
                StaticConfigId = config.Id,
                Status = EAssetStatus.Loading
            };

            XLog.InfoFormat("开始加载 UI: {0}", config.Prefab);

            var handle = await config.Prefab.CreateHandleAsync();
            if (handle == null)
            {
                loadUI.Status = EAssetStatus.Failed;
                return loadUI;
            }

            loadUI.AssetHandle = handle;
            loadUI.UIPrefabTemplate = handle.Get<GameObject>();
            if (loadUI.UIPrefabTemplate == null)
            {
                loadUI.AssetHandle.Release();
                loadUI.Status = EAssetStatus.ErrorAndRelease;
                return loadUI;
            }

            var comp = loadUI.UIPrefabTemplate.GetComponent<IUIWindowCtrlBase>();
            if (comp == null)
            {
                loadUI.AssetHandle.Release();
                loadUI.Status = EAssetStatus.ErrorAndRelease;
                return loadUI;
            }

            loadUI.IUICtrlTemplate = comp;
            loadUI.UIPrefabTemplate.SetActive(false);
            loadUI.UIPrefabTemplate.transform.SetParent(UILayerData[EUILayer.None].LayerRoot.transform);

            if (comp is UIWindowCtrlBase windowCtrl)
            {
                var windowConfig = new UIWindowConfig();
                windowConfig.UITypeI = config.UIHandle;
                windowConfig.Layer = windowCtrl.Layer;
                windowConfig.InstanceType = windowCtrl.UIType;
                windowConfig.IsShowMask = windowCtrl.IsShowMask;
                loadUI.Config = windowConfig;


            }
            else
            {
                // TODO: 处理UIWidgetCtrlBase等其他类型
            }

            // 设置为加载完成状态
            loadUI.Status = EAssetStatus.Success;

            loadedUIData.Set(loadUI, config.Prefab, cfgId);

            XLog.InfoFormat("UI加载完成: {0}", cfgId);
            return loadUI;
        }
    }
}