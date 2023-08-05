using System.Collections.Generic;
using Unity.Netcode;

public class GameState : INetworkSerializable {
  public int count;
  //public List<string> strings = new List<string>();
  public List<StringContainer> stringContainers = new List<StringContainer>();

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    serializer.SerializeValue(ref count);
    
    //var stringsCount = strings.Count;
    //serializer.SerializeValue(ref stringsCount);
    //if (serializer.IsReader) {
    //  strings.Clear();
    //  for (int i = 0; i < stringsCount; i++) {
    //    strings.Add("");
    //    var str = strings[i];
    //    serializer.SerializeValue(ref str);
    //    strings[i] = str;
    //  }
    //}
    //else {
    //  for (int i = 0; i < stringsCount; i++) {
    //    var str = strings[i];
    //    serializer.SerializeValue(ref str);
    //  }
    //}

    var stringContainersCount = stringContainers.Count;
    serializer.SerializeValue(ref stringContainersCount);
    if (serializer.IsReader) {
      stringContainers.Clear();
      for (int i = 0; i < stringContainersCount; i++) {
        stringContainers.Add(new StringContainer(""));
        var stringContainer = stringContainers[i];
        stringContainer.NetworkSerialize(serializer);
        stringContainers[i] = stringContainer;
      }
    } else {
      for (int i = 0; i < stringContainersCount; i++) {
        var stringContainer = stringContainers[i];
        stringContainer.NetworkSerialize(serializer);
      }
    }
  }
}
