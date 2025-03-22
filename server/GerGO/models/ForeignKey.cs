namespace GerGO.models
{
    class ForeignKey
    {
        public string Name { get; set; }
        public string RefTableName { get; set; }
        public string RefAttributeName { get; set; }
        public string AttributeName { get; set; }

        public ForeignKey()
        {
            Name = string.Empty;
            RefTableName = string.Empty;
            RefAttributeName = string.Empty;
            AttributeName = string.Empty;
        }

        public ForeignKey(string name, string refTableName, string refAttributeName, string attributeName)
        {
            Name = name;
            RefTableName = refTableName;
            RefAttributeName = refAttributeName;
            AttributeName = attributeName;
        }
    }
}
