public interface IDeviceRunnerRegistry
{
    public void RegisterDevice(IDeviceRunner deviceRunner);
    public IDeviceRunner? GetDeviceRunner(string deviceIp);
    public IReadOnlyCollection<IDeviceRunner> GetAllDeviceRunners();
}
