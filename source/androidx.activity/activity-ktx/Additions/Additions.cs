#nullable enable

using System;
using AndroidX.Core.App;

namespace AndroidX.Activity.Result
{
	public sealed class ActivityResultCallerLauncher<TInput>
	{
		readonly ActivityResultLauncher<TInput> launcher;
		readonly TInput input;

		internal ActivityResultCallerLauncher (ActivityResultLauncher<TInput> launcher, TInput input)
		{
			this.launcher = launcher ?? throw new ArgumentNullException (nameof (launcher));
			this.input = input;
		}

		public void Launch ()
			=> launcher.Launch (input);

		public void Launch (ActivityOptionsCompat? options)
			=> launcher.Launch (input, options);

		public void Unregister ()
			=> launcher.Unregister ();
	}

	public static class ActivityResultCallerKtxTypedExtensions
	{
		public static ActivityResultCallerLauncher<TInput> RegisterForActivityResult<TInput, TResult> (
			this IActivityResultCaller caller,
			Contract.ActivityResultContract<TInput, TResult> contract,
			TInput input,
			Action<TResult?> callback)
		{
			if (caller is null)
				throw new ArgumentNullException (nameof (caller));
			if (contract is null)
				throw new ArgumentNullException (nameof (contract));
			if (callback is null)
				throw new ArgumentNullException (nameof (callback));

			var launcher = caller.RegisterForActivityResult (contract, callback);
			return new ActivityResultCallerLauncher<TInput> (launcher, input);
		}

		public static ActivityResultCallerLauncher<TInput> RegisterForActivityResult<TInput, TResult> (
			this IActivityResultCaller caller,
			Contract.ActivityResultContract<TInput, TResult> contract,
			ActivityResultRegistry registry,
			TInput input,
			Action<TResult?> callback)
		{
			if (caller is null)
				throw new ArgumentNullException (nameof (caller));
			if (contract is null)
				throw new ArgumentNullException (nameof (contract));
			if (registry is null)
				throw new ArgumentNullException (nameof (registry));
			if (callback is null)
				throw new ArgumentNullException (nameof (callback));

			var launcher = caller.RegisterForActivityResult (contract, registry, callback);
			return new ActivityResultCallerLauncher<TInput> (launcher, input);
		}
	}
}
