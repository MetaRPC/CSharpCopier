using MetaRPC.Copier;
using MetaRPC.Copier.Models;
using System.Text.Json;
try {
 var v=Enumerable.Range(0,13).Select(_=>Console.ReadLine()??throw new Exception("Expected 13 configuration lines")).ToArray();
 using var c=new CopierService("https://"+v[0],v[1]);
 if(args[0]=="stream") {
  await c.StreamTradeLogsAsync(args[1],log=>Console.WriteLine(log.GetRawText()));
  return;
 }
 Account AccountAt(int i)=>new(){Type="MT5",User=ulong.Parse(v[i]),Password=v[i+1],Server=v[i+2],Id=v[i+3]};
 object r=args[0] switch {
 "start"=>await c.StartAsync(new(){Master=AccountAt(2),Slave=AccountAt(6),RiskType=v[10],RiskValue=v[11],FixedMasterBalance=v[12],CopySl=true,CopyTp=true}),
 "list"=>await c.ListAsync(),
 "pause"=>await c.PauseAsync(args[1],args[2]=="true"),
 "remove"=>await c.RemoveAsync(args[1]),
 _=>throw new Exception("Unknown operation")};
 Console.WriteLine(JsonSerializer.Serialize(r,r.GetType()));
} catch(Exception e){Console.WriteLine(JsonSerializer.Serialize(new{ok=false,error=e.Message}));Environment.ExitCode=1;}
