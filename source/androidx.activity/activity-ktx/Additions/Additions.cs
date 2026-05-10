#nullable enable

using System;

namespace AndroidX.Activity.Result
{
    public static class ActivityResultCallerKtxTypedExtensions
    {
        public static ActivityResultCallerLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
            this IActivityResultCaller caller,
            AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
            TInput input,
            Action<TResult?> callback)
            => new(AndroidX.Activity.Result.ActivityResultCallerTypedExtensions.RegisterForActivityResult(caller, contract, callback), input);

        public static ActivityResultCallerLauncher<TInput> RegisterForActivityResult<TInput, TResult>(
            this IActivityResultCaller caller,
            AndroidX.Activity.Result.Contract.ActivityResultContract<TInput, TResult> contract,
            ActivityResultRegistry registry,
            TInput input,
            Action<TResult?> callback)
            => new(AndroidX.Activity.Result.ActivityResultCallerTypedExtensions.RegisterForActivityResult(caller, contract, registry, callback), input);
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
