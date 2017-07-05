using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace FastCsv
{
    public static class TypedObjectLoader
    {
        public static Func<Dictionary<string, string>, T> Create<T>() where T : new()
        {
            var typeOfResultObject = typeof (T);

            ParameterExpression dictExpr = Expression.Parameter(typeof(Dictionary<string, string>), "dict");

            var propertyBindings = GetPropertyBindings<T>(typeOfResultObject, dictExpr);

            NewExpression constructorExpr = Expression.New(typeOfResultObject);

            return Expression.Lambda<Func<Dictionary<string, string>, T>>(
                Expression.MemberInit(constructorExpr, propertyBindings), 
                dictExpr)
                .Compile();
        }

        private static List<MemberBinding> GetPropertyBindings<T>(Type typeOfResultObject, ParameterExpression dictExpr)
        {
            List<MemberBinding> propertyBindings = new List<MemberBinding>();

            var writeableProperties =
                typeOfResultObject.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            foreach (var property in writeableProperties)
            {
                var indexExpr = GetDictionaryIndexExpression(property.Name, dictExpr);
                var propertyMethod = property.GetSetMethod();

                if (property.PropertyType == typeof (string))
                {
                    propertyBindings.Add(Expression.Bind(propertyMethod, indexExpr));
                    continue;
                }

                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                /* Nullable property */
                if (underlyingType != null)
                {
                    MethodInfo parseMethod = GetInvariantParseMethod(underlyingType);
                    propertyBindings.Add(
                        Expression.Bind(propertyMethod,
                            Expression.Condition(
                                Expression.Equal(indexExpr, Expression.Constant(string.Empty)),
                                Expression.Default(property.PropertyType),
                                Expression.Convert(
                                    Expression.Call(parseMethod, indexExpr, Expression.Constant(CultureInfo.InvariantCulture)), property.PropertyType))));
                }
                /* Not nullable property */
                else
                {
                    MethodInfo parseMethod = GetInvariantParseMethod(property.PropertyType);
                    propertyBindings.Add(
                        Expression.Bind(propertyMethod,
                            Expression.Condition(
                                Expression.Equal(indexExpr, Expression.Constant(string.Empty)), 
                                Expression.Default(property.PropertyType),
                                Expression.Call(parseMethod, indexExpr, Expression.Constant(CultureInfo.InvariantCulture)))));
                }
            }
            return propertyBindings;
        }

        private static MethodInfo GetInvariantParseMethod(Type type)
        {
            return type.GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });
        }

        private static IndexExpression GetDictionaryIndexExpression(string propertyName, ParameterExpression dictExpr)
        {
            ConstantExpression keyExpr = Expression.Constant(propertyName);
            PropertyInfo indexer = dictExpr.Type.GetProperty("Item");
            IndexExpression indexExpr = Expression.Property(dictExpr, indexer, keyExpr);
            return indexExpr;
        }
    }
}