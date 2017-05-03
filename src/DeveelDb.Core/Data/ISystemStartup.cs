using System;
using System.Collections.Generic;
using System.Text;

namespace Deveel.Data {
	public interface ISystemStartup {
		void Configure(ISystemBuilder builder);
	}
}