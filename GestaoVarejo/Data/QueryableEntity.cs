using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace GestaoVarejo;

public abstract class QueryableEntity : IEntity
{
    public abstract int Id { get; set; }

    public static string TableName<T>() where T : QueryableEntity 
        => typeof(T).GetCustomAttribute<TableAttribute>()!.Name;

    public void FillValues(string[] values)
    {
         var properties = this.GetType().GetProperties();
        for (int i = 0; i < properties.Length; i++)
        {
            if (i >= values.Length)
                break;

            var property = properties[i];
            var propertyType = property.PropertyType;
            string stringValue = values[i];
            object value = null!;

            if (Nullable.GetUnderlyingType(propertyType) != null && !string.IsNullOrEmpty(stringValue))
            {
                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                if (underlyingType == typeof(int))
                {
                    value = int.Parse(stringValue);
                }
                else if (underlyingType == typeof(decimal))
                {
                    value = decimal.Parse(stringValue);
                }
                else if (underlyingType == typeof(DateTime))
                {
                    value = DateTime.Parse(stringValue);
                }
                else if (propertyType == typeof(string)) 
                {
                    value = stringValue;
                }
            }
            else if (!string.IsNullOrEmpty(stringValue))
            {
                if (propertyType == typeof(int))
                {
                    value = int.Parse(stringValue);
                }
                else if (propertyType == typeof(decimal))
                {
                    value = decimal.Parse(stringValue);
                }
                else if (propertyType == typeof(DateTime))
                {
                    value = DateTime.Parse(stringValue);
                }
                else if (propertyType == typeof(string)) 
                {
                    value = stringValue;
                }
            }

            property.SetValue(this, value);
        }
    }


    public string ToDisplayString()
    {
        var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        foreach (var property in properties)
        {
            var displayAttribute = property.GetCustomAttributes(typeof(DisplayAttribute), true)
                .FirstOrDefault() as DisplayAttribute;

            var displayName = displayAttribute != null ? displayAttribute.Name : property.Name;
            var value = property.GetValue(this, null);

            // Verifica se o valor é um DateTime ou DateTime nullable
            if (value is DateTime dateValue)
            {
                value = dateValue.ToString("yyyy-MM-dd");  // Formata a data como "yyyy-MM-dd"
            }
            else if (value == null)
            {
                value = "null";
            }

                sb.Append($"{displayName}: {value}, ");
            }

        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 2, 2);
        }

        return sb.ToString();
    }
}