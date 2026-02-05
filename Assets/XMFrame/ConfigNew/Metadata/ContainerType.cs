namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// 容器类型枚举
    /// </summary>
    public enum EContainerType
    {
        /// <summary>非容器类型(标量)</summary>
        None = 0,
        
        /// <summary>List&lt;T&gt; 有序列表</summary>
        List = 1,
        
        /// <summary>Dictionary&lt;K,V&gt; 键值对</summary>
        Dictionary = 2,
        
        /// <summary>HashSet&lt;T&gt; 无序集合</summary>
        HashSet = 3,
        
        /// <summary>XBlobMultiMap&lt;K,V&gt; 多值HashMap(预留,暂不支持)</summary>
        MultiValueHashMap = 4
    }
    
    /// <summary>
    /// 索引类型枚举
    /// </summary>
    public enum EIndexType
    {
        /// <summary>非索引字段</summary>
        None = 0,
        
        /// <summary>唯一索引(一对一)</summary>
        Unique = 1,
        
        /// <summary>多值索引(一对多)</summary>
        MultiValue = 2
    }
}
