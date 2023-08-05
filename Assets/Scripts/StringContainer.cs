using Unity.Collections;
using Unity.Netcode;

public class StringContainer : INetworkSerializable {
  public FixedString64Bytes value;

  public StringContainer(string value) {
    this.value = value;
  }

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref value);
  }
}
