using Grpc.Core;
using MetaRPC.Copier;
using MetaRPC.Copier.Models;
using System.Text.Json;
static void Check(bool ok) { if (!ok) throw new Exception("Contract assertion failed"); }
foreach (var key in new[]{"", "TRIAL", "trial", "bad key", "x\r\nInjected: yes"}) {
    try { using var invalid = new CopierAccount("https://copy.mrpc.pro",key); throw new Exception("Accepted bad key"); }
    catch(ArgumentException) { }
}
using var account = new CopierAccount("https://copy.mrpc.pro","owner-test","manager-test");
var spy = new Spy();
using var service = new CopierService(account,spy);
Check((await service.StartAsync(new StartRequest { CopySl=false, CopyTp=true })).Ok);
var list=await service.ListAsync();Check(!list.Ok && list.Error=="refused");
Check((await service.PauseAsync("test",false)).Ok);
Check((await service.RemoveAsync("test")).Ok);
Check(spy.Calls.Count==4);
var req=(tradecopy.Grpc.StartRequest)spy.Calls[0];
Check(req.UserKey=="owner-test" && req.ManagerKey=="manager-test" && !req.CopySl && req.CopyTp);
using var doc=JsonDocument.Parse("{\"SlaveTicket\":\"9007199254740993\",\"Success\":false,\"ExceptionCode\":\"TEST\"}");
var log=TradeLog.FromJson(doc.RootElement);
Check(log.SlaveTicket==9007199254740993 && !log.Success && log.Raw.GetProperty("ExceptionCode").GetString()=="TEST");
try {
 using var invalidDemo=DemoAccountClient.ParseReply("{\"error\":{\"errorCode\":\"MAX_TERMINALS_PER_POD_REACHED\",\"stackTrace\":\"private trace\"}}");
 throw new Exception("HTTP 200 error accepted");
} catch(InvalidOperationException e) { Check(e.Message.Contains("MAX_TERMINALS_PER_POD_REACHED") && !e.Message.Contains("private trace")); }
using var emptyPositions=DemoAccountClient.ParseReply("{\"data\":{\"positionInfos\":[]}}");
Check(emptyPositions.RootElement.GetProperty("data").GetProperty("positionInfos").GetArrayLength()==0);
Console.WriteLine("PASS: RPC authentication, deadlines, options, errors, log payloads and HTTP 200 error envelopes");
class Spy : CallInvoker {
 public List<object> Calls=new();
 public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest,TResponse>(Method<TRequest,TResponse> method,string host,CallOptions options,TRequest request) {
   if(options.Headers.Single(x=>x.Key=="apikey").Value!="owner-test" || options.Deadline==null) throw new Exception("Missing auth/deadline");
   Calls.Add(request);
   object reply=method.Name switch { "Start"=>new tradecopy.Grpc.StartReply{Ok=true,CopierId="test"}, "List"=>new tradecopy.Grpc.ListReply{Ok=false,Error="refused"}, _=>new tradecopy.Grpc.SimpleReply{Ok=true} };
   return new AsyncUnaryCall<TResponse>(Task.FromResult((TResponse)reply),Task.FromResult(new Metadata()),()=>Status.DefaultSuccess,()=>new Metadata(),()=>{});
 }
 public override TResponse BlockingUnaryCall<TRequest,TResponse>(Method<TRequest,TResponse> m,string h,CallOptions o,TRequest r)=>throw new NotSupportedException();
 public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest,TResponse>(Method<TRequest,TResponse> m,string h,CallOptions o,TRequest r)=>throw new NotSupportedException();
 public override AsyncClientStreamingCall<TRequest,TResponse> AsyncClientStreamingCall<TRequest,TResponse>(Method<TRequest,TResponse> m,string h,CallOptions o)=>throw new NotSupportedException();
 public override AsyncDuplexStreamingCall<TRequest,TResponse> AsyncDuplexStreamingCall<TRequest,TResponse>(Method<TRequest,TResponse> m,string h,CallOptions o)=>throw new NotSupportedException();
}
