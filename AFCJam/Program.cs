using iMobileDevice;
using iMobileDevice.Afc;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;

using System;
using System.Collections.ObjectModel;
using System.IO;

namespace AFCJam
{
    internal class Program
    {
		private static string deviceUDID;

		private static readonly IiDeviceApi idevice = LibiMobileDevice.Instance.iDevice;
		private static readonly ILockdownApi lockdown = LibiMobileDevice.Instance.Lockdown;
		private static readonly IAfcApi afc = LibiMobileDevice.Instance.Afc;

		private static iDeviceHandle deviceHandle;
		private static LockdownClientHandle lockdownHandle;
		private static AfcClientHandle afcClientHandle;

		static void Main(string[] args)
        {
			Console.WriteLine("AFCJam by KawaiiZenbo");
			int j;
			if (args.Length > 0) j = Jam(args[0]);
			else j = 1;
			if (j != 0) Console.WriteLine($"failed with error code {j} :(");
			else Console.WriteLine("complete :)");
			Console.ReadKey();
		}

		private static int Jam(string filePath)
        {
			if (!File.Exists(filePath)) return 1;
			int count = 0;
            if (idevice.idevice_get_device_list(out ReadOnlyCollection<string> devices, ref count) == iDeviceError.NoDevice || count == 0) return 2;
            deviceUDID = devices[0];
			if (idevice.idevice_new(out deviceHandle, deviceUDID) > iDeviceError.Success) return 3;
			if (lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "AFCJam") > LockdownError.Success) return 4;
			if (lockdown.lockdownd_get_device_name(lockdownHandle, out string deviceName) > LockdownError.Success) return 5;
			Console.WriteLine($"Connected to {deviceName}");
			if (afc.afc_client_start_service(deviceHandle, out afcClientHandle, "AFCJam") > AfcError.Success) return 6;
			ulong handle = 0;
			if (afc.afc_file_open(afcClientHandle, "/" + Path.GetFileName(filePath), AfcFileMode.FopenRw, ref handle) > AfcError.Success) return 7;
			byte[] array = File.ReadAllBytes(filePath); 
			uint written = 0;
			if (afc.afc_file_write(afcClientHandle, handle, array, (uint)array.Length, ref written) > AfcError.Success) return 8;
			if (afc.afc_file_close(afcClientHandle, handle) > AfcError.Success) return 9;
			return 0;
		}
	}
}
