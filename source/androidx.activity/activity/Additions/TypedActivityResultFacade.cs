#nullable enable

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using AndroidX.Core.App;
using Java.Interop;

namespace AndroidX.Activity.Result
{
internal static class ActivityResultFacadeMarshal
{
    public static Java.Lang.Object? ToJavaObject<T>(T? value)
    {
        if (value is null)
            return null;

        if (value is Java.Lang.Object javaObject)
            return javaObject;

        object boxed = value;
        return boxed switch
        {
            string stringValue => new Java.Lang.String(stringValue),
            bool boolValue => Java.Lang.Boolean.ValueOf(boolValue),
            int intValue => Java.Lang.Integer.ValueOf(intValue),
            long longValue => Java.Lang.Long.ValueOf(longValue),
            float floatValue => Java.Lang.Float.ValueOf(floatValue),
            double doubleValue => Java.Lang.Double.ValueOf(doubleValue),
            string[] stringArrayValue => Java.Lang.Object.GetObject<Java.Lang.Object>(JNIEnv.NewArray(stringArrayValue), JniHandleOwnership.TransferLocalRef),
            _ => throw new InvalidCastException($"Cannot marshal type '{boxed.GetType().FullName}' to Java.Lang.Object."),
        };
    }

    public static T? FromJavaObject<T>(Java.Lang.Object? value)
    {
        if (value is null)
            return default;

        if (value is T typed)
            return typed;

        object? converted = TryConvertFromJavaObject(typeof(T), value);
        if (converted is T convertedTyped)
            return convertedTyped;

        throw new InvalidCastException($"Cannot convert Java value '{value.GetType().FullName}' to '{typeof(T).FullName}'.");
    }

    static object? TryConvertFromJavaObject(Type targetType, Java.Lang.Object value)
    {
        var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (effectiveType == typeof(string))
            return value.ToString();

        if (effectiveType == typeof(bool))
            return (value as Java.Lang.Boolean)?.BooleanValue();

        if (effectiveType == typeof(int))
            return (value as Java.Lang.Integer)?.IntValue();

        if (effectiveType == typeof(long))
            return (value as Java.Lang.Long)?.LongValue();

        if (effectiveType == typeof(float))
            return (value as Java.Lang.Float)?.FloatValue();

        if (effectiveType == typeof(double))
            return (value as Java.Lang.Double)?.DoubleValue();

        if (effectiveType == typeof(IDictionary<string, bool>))
        {
            var javaDictionary = Android.Runtime.JavaDictionary<string, Java.Lang.Boolean>.FromJniHandle(value.Handle, JniHandleOwnership.DoNotTransfer);
            var dictionary = new Dictionary<string, bool>(javaDictionary.Count);
            foreach (var pair in javaDictionary)
                dictionary[pair.Key] = pair.Value.BooleanValue();
            return dictionary;
        }

        if (effectiveType == typeof(IList<Android.Net.Uri>))
            return Android.Runtime.JavaList<Android.Net.Uri>.FromJniHandle(value.Handle, JniHandleOwnership.DoNotTransfer);

        return null;
    }
}
}

namespace AndroidX.Activity.Result.Contract
{

public partial class ActivityResultContract
{
    public sealed class SynchronousResult<TResult>
    {
        public SynchronousResult(TResult? value)
        {
            Value = value;
        }

        public TResult? Value { get; }

        internal ActivityResultContract.SynchronousResult ToRaw()
            => new ActivityResultContract.SynchronousResult(ActivityResultFacadeMarshal.ToJavaObject(Value));

        internal static SynchronousResult<TResult>? FromRaw(ActivityResultContract.SynchronousResult? raw)
            => raw is null ? null : new SynchronousResult<TResult>(ActivityResultFacadeMarshal.FromJavaObject<TResult>(raw.Value));
    }
}

public sealed class ActivityResultContract<TInput, TResult>
{
    readonly Func<TInput?, Java.Lang.Object?> inputMarshaler;
    readonly Func<Java.Lang.Object?, TResult?> outputMarshaler;

    public ActivityResultContract(ActivityResultContract rawContract)
        : this(rawContract, ActivityResultFacadeMarshal.ToJavaObject, ActivityResultFacadeMarshal.FromJavaObject<TResult>)
    {
    }

    public ActivityResultContract(
        ActivityResultContract rawContract,
        Func<TInput?, Java.Lang.Object?> inputMarshaler,
        Func<Java.Lang.Object?, TResult?> outputMarshaler)
    {
        RawContract = rawContract ?? throw new ArgumentNullException(nameof(rawContract));
        this.inputMarshaler = inputMarshaler ?? throw new ArgumentNullException(nameof(inputMarshaler));
        this.outputMarshaler = outputMarshaler ?? throw new ArgumentNullException(nameof(outputMarshaler));
    }

    public ActivityResultContract RawContract { get; }

    internal Java.Lang.Object? MarshalInput(TInput? input)
        => inputMarshaler(input);

    internal TResult? MarshalOutput(Java.Lang.Object? output)
        => outputMarshaler(output);

    public Intent CreateIntent(Context context, TInput? input)
        => RawContract.CreateIntent(context, inputMarshaler(input));

    public TResult? ParseResult(int resultCode, Intent? intent)
        => outputMarshaler(RawContract.ParseResult(resultCode, intent));

    public ActivityResultContract.SynchronousResult<TResult>? GetSynchronousResult(Context context, TInput? input)
        => ActivityResultContract.SynchronousResult<TResult>.FromRaw(RawContract.GetSynchronousResult(context, inputMarshaler(input)));
}

public static class ActivityResultContractTypedExtensions
{
    public static ActivityResultContract<TInput, TResult> AsTyped<TInput, TResult>(this ActivityResultContract contract)
        => new ActivityResultContract<TInput, TResult>(contract);
}
}

namespace AndroidX.Activity.Result
{

public interface IActivityResultCallback<TResult>
{
    void OnActivityResult(TResult? result);
}

sealed class ActivityResultCallbackAdapter<TResult> : Java.Lang.Object, global::AndroidX.Activity.Result.IActivityResultCallback
{
    readonly Func<Java.Lang.Object?, TResult?> outputMarshaler;
    readonly Action<TResult?> callback;

    public ActivityResultCallbackAdapter(Action<TResult?> callback, Func<Java.Lang.Object?, TResult?> outputMarshaler)
    {
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        this.outputMarshaler = outputMarshaler ?? throw new ArgumentNullException(nameof(outputMarshaler));
    }

    public void OnActivityResult(Java.Lang.Object? result)
        => callback(outputMarshaler(result));
}

public sealed class ActivityResultCallback<TResult> : Java.Lang.Object, global::AndroidX.Activity.Result.IActivityResultCallback, IActivityResultCallback<TResult>
{
    readonly Action<TResult?> callback;

    public ActivityResultCallback(Action<TResult?> callback)
    {
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public void OnActivityResult(TResult? result)
        => callback(result);

    void global::AndroidX.Activity.Result.IActivityResultCallback.OnActivityResult(Java.Lang.Object? result)
        => callback(ActivityResultFacadeMarshal.FromJavaObject<TResult>(result));
}

public sealed class ActivityResultLauncher<TInput>
{
    readonly Func<TInput?, Java.Lang.Object?> inputMarshaler;

    internal ActivityResultLauncher(global::AndroidX.Activity.Result.ActivityResultLauncher rawLauncher, Func<TInput?, Java.Lang.Object?> inputMarshaler)
    {
        RawLauncher = rawLauncher ?? throw new ArgumentNullException(nameof(rawLauncher));
        this.inputMarshaler = inputMarshaler ?? throw new ArgumentNullException(nameof(inputMarshaler));
    }

    public global::AndroidX.Activity.Result.ActivityResultLauncher RawLauncher { get; }

    public global::AndroidX.Activity.Result.Contract.ActivityResultContract RawContract
        => RawLauncher.RawContract;

    public void Launch(TInput? input)
        => RawLauncher.Launch(inputMarshaler(input));

    public void Launch(TInput? input, ActivityOptionsCompat? options)
        => RawLauncher.Launch(inputMarshaler(input), options);

    public void Unregister()
        => RawLauncher.Unregister();
}

public static class TypedActivityResultExtensions
{
    public static ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
        this IActivityResultCaller caller,
        global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
        IActivityResultCallback<TResult> callback)
    {
        if (caller is null)
            throw new ArgumentNullException(nameof(caller));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var rawCallback = new ActivityResultCallbackAdapter<TResult>(callback.OnActivityResult, contract.MarshalOutput);
        var launcher = caller.RegisterForActivityResult(contract.RawContract, rawCallback);
        return new ActivityResultLauncher<TInput>(launcher, contract.MarshalInput);
    }

    public static ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
        this IActivityResultCaller caller,
        global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
        Action<TResult?> callback)
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return RegisterForActivityResult(caller, contract, new ActivityResultCallback<TResult>(callback));
    }

    public static ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
        this IActivityResultCaller caller,
        global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
        ActivityResultRegistry registry,
        IActivityResultCallback<TResult> callback)
    {
        if (caller is null)
            throw new ArgumentNullException(nameof(caller));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var rawCallback = new ActivityResultCallbackAdapter<TResult>(callback.OnActivityResult, contract.MarshalOutput);
        var launcher = caller.RegisterForActivityResult(contract.RawContract, registry, rawCallback);
        return new ActivityResultLauncher<TInput>(launcher, contract.MarshalInput);
    }

    public static ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
        this IActivityResultCaller caller,
        global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
        ActivityResultRegistry registry,
        Action<TResult?> callback)
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return RegisterForActivityResult(caller, contract, registry, new ActivityResultCallback<TResult>(callback));
    }

    public static ActivityResultLauncher<TInput> Register<TInput, TResult>(
        this ActivityResultRegistry registry,
        string key,
        global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
        IActivityResultCallback<TResult> callback)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var rawCallback = new ActivityResultCallbackAdapter<TResult>(callback.OnActivityResult, contract.MarshalOutput);
        var launcher = registry.Register(key, contract.RawContract, rawCallback);
        return new ActivityResultLauncher<TInput>(launcher, contract.MarshalInput);
    }

    public static ActivityResultLauncher<TInput> Register<TInput, TResult>(
        this ActivityResultRegistry registry,
        string key,
        global::AndroidX.Lifecycle.ILifecycleOwner lifecycleOwner,
        global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
        IActivityResultCallback<TResult> callback)
    {
        if (registry is null)
            throw new ArgumentNullException(nameof(registry));
        if (lifecycleOwner is null)
            throw new ArgumentNullException(nameof(lifecycleOwner));
        if (contract is null)
            throw new ArgumentNullException(nameof(contract));
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var rawCallback = new ActivityResultCallbackAdapter<TResult>(callback.OnActivityResult, contract.MarshalOutput);
        var launcher = registry.Register(key, lifecycleOwner, contract.RawContract, rawCallback);
        return new ActivityResultLauncher<TInput>(launcher, contract.MarshalInput);
    }

    public static ActivityResultLauncher<TInput> AsTyped<TInput>(
        this global::AndroidX.Activity.Result.ActivityResultLauncher launcher,
        Func<TInput?, Java.Lang.Object?>? inputMarshaler = null)
    {
        if (launcher is null)
            throw new ArgumentNullException(nameof(launcher));

        return new ActivityResultLauncher<TInput>(launcher, inputMarshaler ?? ActivityResultFacadeMarshal.ToJavaObject);
    }
}
}
