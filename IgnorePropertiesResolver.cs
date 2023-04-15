using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

//short helper class to ignore some properties from serialization
public class IgnorePropertiesResolver : DefaultContractResolver
{
    private readonly HashSet<string> ignoreProps;
    public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
    {
        this.ignoreProps = new HashSet<string>(propNamesToIgnore);
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        if (this.ignoreProps.Contains(property.PropertyName))
        {
            property.ShouldSerialize = _ => false;
        }
        return property;
    }
}

/*** Sample Usage: When we want to serialize Vector3 or Vector2 where it has normalized (property) which is the same class as its struct.
  
    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        ContractResolver = new IgnorePropertiesResolver(new List<string> { "name", "hideFlags", "normalized", "magnitude", "sqrMagnitude" }),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };


***/