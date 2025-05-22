using UnityEngine;
using System.Collections.Generic;

namespace ProjectCI.CoreSystem.Runtime.Attributes
{
    [CreateAssetMenu(fileName = "AttributeTypeDefinition", menuName = "ProjectCI/Attributes/Create AttributeTypeDefinition")]
    public class AttributeTypeDefinition : ScriptableObject
    {
        [SerializeField]
        private List<string> attributeTypeNames = new List<string>();

        public IReadOnlyList<string> AttributeTypeNames => attributeTypeNames;
    }
} 