using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using XMFrame.Interfaces;

namespace XMFrame.Implementation
{
    public enum ELoadBoxStatus
    {
        None = 0,
        Loading = 1,
        Loaded = 2,
        Destroyed = 3
    }

    public class UILoadBoxParam
    {
        public XAssetId? BasicAssetId;
        public int? Count;
        public Canvas Canvas;
        public GameObject ParentGameObject;
        public List<UICtrlBase> LoadedObjs;
        public UILoadBox LoadBox;
        public Action<IReadOnlyList<UICtrlBase>> OnLoadSuccess;
        public UICtrlBase ParentCtrl;
        public ELoadBoxStatus Status;
        public Dictionary<int,XAssetId?> SpecialAssetIdDic;


        public UILoadBoxParam SetAssetId(XAssetId assetId)
        {
            if (assetId.Valid)
            {
                BasicAssetId = assetId;
            }
            else
            {
                return null;
            }

            return this;
        }
        
        
        
        public UILoadBoxParam SetAssetPath(XAssetPath xAssetId)
        {
            var assetId = xAssetId.GetAssetId();
            if (assetId.Valid)
            {
                BasicAssetId = assetId;
            }
            else
            {
                return null;
            }

            return this;
        }

        public UILoadBoxParam SetLoadCount(int count)
        {
            Count = count;
            return this;
        }

        public UILoadBoxParam SetParent(GameObject parent)
        {
            ParentGameObject = parent;
            return this;
        }

        public UILoadBoxParam SetCanvas(Canvas canvas)
        {
            Canvas = canvas;
            return this;
        }

        public UILoadBoxParam SetCallBack(Action<IReadOnlyList<UICtrlBase>> onLoadSuccess)
        {
            OnLoadSuccess = onLoadSuccess;
            return this;
        }

        /// <summary>
        /// 异步加载UI控制器并返回组件列表
        /// </summary>
        private async UniTask<List<UICtrlBase>> LoadUICtrlAsync(XAssetId assetId, int count)
        {
            var instances = ListPool<UICtrlBase>.Get();
            var uim = IUIManager.I as UIManager;
            if (uim != null)
            {
                await uim.CreateUICtrlByConfigWithInstances(assetId, count, instances);
            }
            return instances;
        }

        public async UniTask<IReadOnlyList<UICtrlBase>> StartLoad(UICtrlBase parentCtrlInstance)
        {
            // 检查父控制器是否为空
            if (parentCtrlInstance == null)
            {
                return null;
            }

            ParentCtrl = parentCtrlInstance;
            var loadBox = ParentGameObject.GetComponent<UILoadBox>();
            
            // 卸载旧的
            if (loadBox != null)
            {
                loadBox.Param.Release();
                if (BasicAssetId == null || BasicAssetId.Value.Valid == false)
                {
                    BasicAssetId = loadBox.xAsset.GetAssetId();
                }

                if (Count is null or <= 0)
                {
                    Count = loadBox.LoadCount;
                }
            }
            else
            {
                if (BasicAssetId == null || BasicAssetId.Value.Valid == false || Count == null || Count <= 0)
                {
                    return null;
                }
                loadBox = ParentGameObject.AddComponent<UILoadBox>();
            }
            
            if (BasicAssetId == null || BasicAssetId.Value.Valid == false || Count == null || Count <= 0)
            {
                return null;
            }
            
            loadBox.Param = this;
            LoadBox = loadBox;
            Status = ELoadBoxStatus.Loading;
            
            // 初始化LoadedObjs列表（现在是UICtrlBase列表）
            if (LoadedObjs == null)
            {
                LoadedObjs = ListPool<UICtrlBase>.Get();
            }
            else
            {
                LoadedObjs.Clear();
            }
            
            // 创建UI组件列表
            var uiComponents = ListPool<UICtrlBase>.Get();
            
            // 检查是否有特殊资源ID字典
            if (SpecialAssetIdDic != null && SpecialAssetIdDic.Count > 0)
            {
                // 先规划任务：统计所有需要加载的资源ID和数量，同时记录每个索引对应的资源ID
                var loadTasks = DictionaryPool<XAssetId, int>.Get();
                var indexToAssetId = DictionaryPool<int, XAssetId>.Get();
                
                // 遍历0到Count-1，记录每个索引对应的资源ID
                for (int index = 0; index < Count.Value; index++)
                {
                    XAssetId assetIdToUse;
                    
                    // 检查字典中是否存在该索引（注意：字典中的key可能是1-based）
                    int dictKey = index + 1;
                    if (SpecialAssetIdDic.TryGetValue(dictKey, out XAssetId? specialAssetId) 
                        && specialAssetId.HasValue 
                        && specialAssetId.Value.Valid)
                    {
                        // 使用特殊资源ID
                        assetIdToUse = specialAssetId.Value;
                    }
                    else
                    {
                        // 使用BasicAssetId
                        assetIdToUse = BasicAssetId.Value;
                    }
                    
                    // 记录索引对应的资源ID
                    indexToAssetId[index] = assetIdToUse;
                    
                    // 统计每个资源ID需要创建的数量
                    if (!loadTasks.TryAdd(assetIdToUse, 1))
                    {
                        loadTasks[assetIdToUse]++;
                    }
                }
                
                // 并行加载所有资源，获得结果：assetId -> List<UICtrlBase>
                var taskList = ListPool<UniTask<List<UICtrlBase>>>.Get();
                var assetIdToTaskIndex = DictionaryPool<XAssetId, int>.Get();
                int taskIndex = 0;
                
                foreach (var kvp in loadTasks)
                {
                    var assetId = kvp.Key;
                    var count = kvp.Value;
                    
                    // 记录资源ID对应的任务索引
                    assetIdToTaskIndex[assetId] = taskIndex++;
                    
                    // 创建加载任务
                    taskList.Add(LoadUICtrlAsync(assetId, count));
                }
                
                // 等待所有加载任务完成
                var results = await UniTask.WhenAll(taskList);
                
                // 构建结果字典：assetId -> List<UICtrlBase>
                var assetIdToComponents = DictionaryPool<XAssetId, List<UICtrlBase>>.Get();
                var assetIdToConsumedIndex = DictionaryPool<XAssetId, int>.Get();
                
                foreach (var kvp in loadTasks)
                {
                    var assetId = kvp.Key;
                    var taskIdx = assetIdToTaskIndex[assetId];
                    assetIdToComponents[assetId] = results[taskIdx] ?? ListPool<UICtrlBase>.Get();
                    assetIdToConsumedIndex[assetId] = 0;
                }
                
                // 从0到Count-1遍历，根据索引对应的资源ID，从结果中取出组件并赋值
                for (int index = 0; index < Count.Value; index++)
                {
                    var assetId = indexToAssetId[index];
                    var componentList = assetIdToComponents[assetId];
                    var consumedIndex = assetIdToConsumedIndex[assetId];
                    
                    // 从结果中取出一个组件
                    if (consumedIndex < componentList.Count)
                    {
                        var component = componentList[consumedIndex];
                        if (component != null)
                        {
                            uiComponents.Add(component);
                            assetIdToConsumedIndex[assetId] = consumedIndex + 1;
                        }
                    }
                }
                
                // 释放临时字典
                DictionaryPool<XAssetId, int>.Release(loadTasks);
                DictionaryPool<int, XAssetId>.Release(indexToAssetId);
                DictionaryPool<XAssetId, int>.Release(assetIdToTaskIndex);
                DictionaryPool<XAssetId, int>.Release(assetIdToConsumedIndex);
                
                // 释放组件字典（注意：组件列表来自 results，会在后面统一释放）
                DictionaryPool<XAssetId, List<UICtrlBase>>.Release(assetIdToComponents);
                
                // 释放 results 中的列表
                foreach (var result in results)
                {
                    if (result != null)
                    {
                        ListPool<UICtrlBase>.Release(result);
                    }
                }
                
                // 释放任务列表
                ListPool<UniTask<List<UICtrlBase>>>.Release(taskList);
            }
            else
            {
                // 使用BasicAssetId创建指定数量的UI
                var createdInstances = ListPool<UICtrlBase>.Get();
                var uim = IUIManager.I as UIManager;
                if (uim != null)
                {
                    await uim.CreateUICtrlByConfigWithInstances(BasicAssetId.Value, Count.Value, createdInstances);
                    uiComponents.AddRange(createdInstances);
                    ListPool<UICtrlBase>.Release(createdInstances);
                }
            }
            
            // 将结果赋值给LoadedObjs
            LoadedObjs.AddRange(uiComponents);
            Status = ELoadBoxStatus.Loaded;
            
            // 调用回调函数
            OnLoadSuccess?.Invoke(uiComponents);
            
            // 释放uiComponents（注意：LoadedObjs会保留引用，所以不能释放）
            // uiComponents 会在 Release 方法中释放
            ListPool<UICtrlBase>.Release(uiComponents);
            
            return LoadedObjs;
        }

        private void Release()
        {
            if (ParentCtrl != null && LoadBox != null)
            {
                ParentCtrl.DynamicLoadBox.Remove(LoadBox);
            }

            if (LoadedObjs != null)
            {
                // 直接释放所有UI组件
                foreach (var component in LoadedObjs)
                {
                    if (component != null)
                    {
                        IUIManager.I.ReleaseUICtrl(component);
                    }
                }
                
                // 释放回池
                ListPool<UICtrlBase>.Release(LoadedObjs);
                LoadedObjs = null;
            }
        }
    }

    [DisallowMultipleComponent]
    public class UILoadBox : MonoBehaviour
    {
        public XAssetPath xAsset;
        public int LoadCount;

        public UILoadBoxParam Param;
    }
}