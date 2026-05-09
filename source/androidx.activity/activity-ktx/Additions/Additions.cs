#nullable enable

using System;

namespace AndroidX.Activity.Result
{
    public static class ActivityResultCallerKtxTypedExtensions
    {
        public static global::AndroidX.Activity.Result.ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
            this IActivityResultCaller caller,
            global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
            Action<TResult?> callback)
            => contract.RegisterForActivityResult(caller, callback);

        public static global::AndroidX.Activity.Result.ActivityResultLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
            this IActivityResultCaller caller,
            global::AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
            ActivityResultRegistry registry,
            Action<TResult?> callback)
            => contract.RegisterForActivityResult(caller, registry, callback);
    }
}

namespace AndroidX.Activity.Result.Contract
{
    public static class ActivityResultContractKtxTypedExtensions
    {
        public static ActivityResultContract<TInput, TResult> AsTyped<TInput, TResult>(this ActivityResultContract contract)
            => new(contract);
    }
}
