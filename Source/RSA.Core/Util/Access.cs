using System;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace RSA.Core.Util {
    // totally incomplete set of accessors for multiple things
    public static class Access {

        /// <summary>
        /// Get's a <em>fast</em> accessor to a an <see cref="BindingFlags.NonPublic"/> <see cref="BindingFlags.Instance"/> property value.
        /// </summary>
        /// <typeparam name="T">Type of the owning instance</typeparam>
        /// <typeparam name="P">Type of the property</typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <remarks>This access if orders of magnitude faster than using Reclection.</remarks>
        public static Func<T, P> GetPropertyGetter<T, P>([NotNull] string propertyName) {
            MethodInfo mi = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);
            DynamicMethod dm = new DynamicMethod(String.Empty, typeof(P), new[] { typeof(T) }, typeof(T));
            var ilGen = dm.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Callvirt, mi);
            ilGen.Emit(OpCodes.Ret);

            return (Func<T, P>)dm.CreateDelegate(typeof(Func<T, P>));
        }

        /// <summary>
        /// Get's a <em>fast</em> accessor to a an <see cref="BindingFlags.NonPublic"/> <see cref="BindingFlags.Static"/> property value.
        /// </summary>
        /// <typeparam name="P">Type of the property</typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="ownerType">Owning Type</param>
        /// <remarks>This access if orders of magnitude faster than using Reclection.</remarks>
        public static Func<P> GetPropertyGetter<P>([NotNull] string propertyName, [NotNull] Type ownerType) {
            MethodInfo mi = ownerType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
            DynamicMethod dm = new DynamicMethod(String.Empty, typeof(P), Type.EmptyTypes, ownerType);
            var ilGen = dm.GetILGenerator();
            ilGen.Emit(OpCodes.Callvirt, mi);
            ilGen.Emit(OpCodes.Ret);

            return (Func<P>)dm.CreateDelegate(typeof(Func<P>));
        }

        /// <summary>
        /// Get's a <em>fast</em> accessor to a an <see cref="BindingFlags.NonPublic"/> <see cref="BindingFlags.Instance"/> field value.
        /// </summary>
        /// <typeparam name="T">Type of the owning instance</typeparam>
        /// <typeparam name="F">Type of the field</typeparam>
        /// <param name="fieldName">Name of the field</param>
        /// <remarks>This access if orders of magnitude faster than using Reclection.</remarks>
        public static Func<T, F> GetFieldGetter<T, F>([NotNull] string fieldName) {
            FieldInfo fi = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            DynamicMethod dm = new DynamicMethod(String.Empty, typeof(F), new[] { typeof(T) }, typeof(T));
            var ilGen = dm.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, fi);
            ilGen.Emit(OpCodes.Ret);

            return (Func<T, F>)dm.CreateDelegate(typeof(Func<T, F>));
        }


        /// <summary>
        /// Get's a <em>fast</em> accessor to a an <see cref="BindingFlags.NonPublic"/> <see cref="BindingFlags.Static"/> field value.
        /// </summary>
        /// <typeparam name="T">Type of the owning instance</typeparam>
        /// <typeparam name="F">Type of the field</typeparam>
        /// <param name="fieldName">Name of the field</param>
        /// <remarks>This access if orders of magnitude faster than using Reclection.</remarks>
        public static Func<F> GetFieldGetter<F>([NotNull] string fieldName, Type ownerType) {
            FieldInfo fi = ownerType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
            DynamicMethod dm = new DynamicMethod(String.Empty, typeof(F),Type.EmptyTypes, ownerType);
            var ilGen = dm.GetILGenerator();
            ilGen.Emit(OpCodes.Ldfld, fi);
            ilGen.Emit(OpCodes.Ret);

            return (Func<F>)dm.CreateDelegate(typeof(Func<F>));
        }
    }
}
