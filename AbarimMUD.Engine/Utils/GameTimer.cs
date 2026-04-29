using System;

namespace AbarimMUD.Utils
{
	public class GameTimer
	{
		private DateTime? _lastDt;

		public int IntervalInMilliseconds { get; set; } = 1000;

		public Action<TimeSpan> Tick;

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

			Tick?.Invoke(elapsed);

			_lastDt = now;
		}
	}
}
