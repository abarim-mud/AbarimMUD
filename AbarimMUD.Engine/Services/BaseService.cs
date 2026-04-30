using System;

namespace AbarimMUD.Services
{
	public abstract class BaseService
	{
		private DateTime? _lastDt;

		public int IntervalInMilliseconds { get; set; } = 1000;

		public void Update()
		{
			var now = DateTime.Now;
			if (_lastDt == null)
			{
				_lastDt = now;
				return;
			}

			var elapsed = now - _lastDt.Value;
			if (elapsed.TotalMilliseconds < IntervalInMilliseconds)
			{
				return;
			}

			InternalUpdate(elapsed);

			_lastDt = now;
		}

		protected abstract void InternalUpdate(TimeSpan elapsed);
	}
}
