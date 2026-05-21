#region Licence
/****************************************************************
 *  Filename: RequestExamples.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.Ocsp;
using Pod.DtoModels;
using Pod.Enums;
using Pod.ViewModels.Auth;
using Pod.ViewModels.Customer;
using Pod.ViewModels.User;
using Swashbuckle.AspNetCore.Filters;

namespace Pod.Web.Center.Swagger.Examples
{
    #region Auth Controller
    public class RequestLoginModelDtoExample : IExamplesProvider<RequestLoginModelDto>
    {
        public RequestLoginModelDto GetExamples()
        {
            return new RequestLoginModelDto()
                   {
                           Username = "jamesjones",
                           Password = "MyPassword-1234"
                   };
        }
    }
    public class RequestTokenRefreshDtoExample : IExamplesProvider<RequestTokenRefreshDto>
    {
        public RequestTokenRefreshDto GetExamples()
        {
            return new RequestTokenRefreshDto()
                   {
                           RefreshToken =
                                   "GfBT1FnxNEl5GhpahoZ8sI5L9m7LlkwoHtUyboFFWwdLC2KKtE+MfpnmWbK12sdkzJ6I1e9EzCaO/FneHQKKhNNsFZiXHreBE/o0XkUDqVPQjErk07IbLGm5z4IYXmeOrbkuRD0hYN9eYAAxj3QGh4tS8I7lo0hGsxZWigpMFLen2+GgrOwu/oZJC7zXvlSrcpbz1C32qh5ns9cJf0bLA8xB13aI8K0Ctu2MBkYyMiF/jRpi"
                   };
        }
    }
    public class LoginResponseViewModelExample : IExamplesProvider<LoginResponseViewModel>
    {
        public LoginResponseViewModel GetExamples()
        {
            return new LoginResponseViewModel()
                   {
                           AccessToken = new AccessTokenViewModel(
                                   "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJzdXBlcnVzZXIiLCJqdGkiOiJmNDNlMDUzMC1iOGUxLTQ2NzYtYmI2Zi0xMmE4YmQxOTNmNTciLCJpYXQiOjE1NTk4NzczMTEsIlVzZXJJZCI6ImJkNTdlMWQxLWVlZTYtNGIzOS04NjVkLWJkY2Y5ODBlNDE1ZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6WyJVc2VyIiwiQWNjb3VudGFudCIsIlNlcnZlck1hbmFnZXIiLCJDdXN0b21lclN1cHBvcnQiLCJBZG1pbmlzdHJhdG9yIl0sIm5iZiI6MTU1OTg3NzMxMSwiZXhwIjoxNTU5ODg0NTExLCJpc3MiOiJ3ZWJBcGkiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAvIn0.TN8thk_6GxjphGzgIBH7w_uYiIPWzUAXVVJ65AWj9ag",
                                   7200),
                           RefreshToken =
                                   "GfBT1FnxNEl5GhpahoZ8sI5L9m7LlkwoHtUyboFFWwdLC2KKtE+MfpnmWbK12sdkzJ6I1e9EzCaO/FneHQKKhNNsFZiXHreBE/o0XkUDqVPQjErk07IbLGm5z4IYXmeOrbkuRD0hYN9eYAAxj3QGh4tS8I7lo0hGsxZWigpMFLen2+GgrOwu/oZJC7zXvlSrcpbz1C32qh5ns9cJf0bLA8xB13aI8K0Ctu2MBkYyMiF/jRpi"
                   };
        }
    }
    public class AccessTokenViewModelExample : IExamplesProvider<AccessTokenViewModel>
    {
        public AccessTokenViewModel GetExamples()
        {
            return new AccessTokenViewModel(
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJzdXBlcnVzZXIiLCJqdGkiOiJmNDNlMDUzMC1iOGUxLTQ2NzYtYmI2Zi0xMmE4YmQxOTNmNTciLCJpYXQiOjE1NTk4NzczMTEsIlVzZXJJZCI6ImJkNTdlMWQxLWVlZTYtNGIzOS04NjVkLWJkY2Y5ODBlNDE1ZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6WyJVc2VyIiwiQWNjb3VudGFudCIsIlNlcnZlck1hbmFnZXIiLCJDdXN0b21lclN1cHBvcnQiLCJBZG1pbmlzdHJhdG9yIl0sIm5iZiI6MTU1OTg3NzMxMSwiZXhwIjoxNTU5ODg0NTExLCJpc3MiOiJ3ZWJBcGkiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUwMDAvIn0.TN8thk_6GxjphGzgIBH7w_uYiIPWzUAXVVJ65AWj9ag",
                    7200);
        }
    }
    #endregion

    #region Account Controller
    public class RequestRegisterUserDtoExample : IExamplesProvider<RequestRegisterUserDto>
    {
        public RequestRegisterUserDto GetExamples()
        {
            return new RequestRegisterUserDto()
                   {
                           Username = "username",
                           EMailAddress = "myemail@example.com",
                           Password = "SecurePassword-1234"
                   };
        }
    }
    public class RequestForgotPasswordDtoExample : IExamplesProvider<RequestForgotPasswordDto>
    {
        public RequestForgotPasswordDto GetExamples()
        {
            return new RequestForgotPasswordDto()
                   {
                           Username = "username"
            };
        }
    }
    public class RequestResendConfirmationEmailDtoExample : IExamplesProvider<RequestResendConfirmationEmailDto>
    {
        public RequestResendConfirmationEmailDto GetExamples()
        {
            return new RequestResendConfirmationEmailDto()
                   {
                           Username = "username"
                   };
        }
    }

    public class RequestResetPasswordDtoExample : IExamplesProvider<RequestResetPasswordDto>
    {
        public RequestResetPasswordDto GetExamples()
        {
            return new RequestResetPasswordDto()
                   {
                           Username = "username",
                           NewPassword = "NewSecurePassword#1234",
                           PasswordResetToken =
                                   "CfDJ8FnxOEl5EhpahoZ8sI5L9m7LlkwoRtZXboFFWwdLC2KKtE+MfpnmWbK13HdkzJ6I1e9EzCaO/FneHQKKhNNsFZiXHreBE/o0XkUDqVPQjErk07IbLGm5z4IYXmeOrbkuRD0hYN9eYAAxj3QGh4tS8I7lo0hGsxZWigpMFLen2+GgrVwu/oZJC7zXvlSrcpbz1C32qh5ns9cJf0bLA8xB13aI8K0Ctu6MBkJyMED/jQnd"
                   };
        }
    }
    public class RequestEmailConfirmationDtoExample : IExamplesProvider<RequestEmailConfirmationDto>
    {
        public RequestEmailConfirmationDto GetExamples()
        {
            return new RequestEmailConfirmationDto()
                   {
                           Username = "username",
                           EmailConfirmationToken =
                                   "AfBJ1FnxXEl5EhpahoZ8sI5L9m7LlkwoRtZXboFFWwdLC2KKtE+MfpnmWbK13HdkzJ6I1e9EzCaO/FneHQKKhNNsFZiXHreBE/o0XkUDqVPQjErk07IbLGm5z4IYXmeOrbkuRD0hYN9eYAAxj3QGh4tS8I7lo0hGsxZWigpMFLen2+GgrVwu/oZJC7zXvlSrcpbz1C32qh5ns9cJf0bLA8xB13aI8K0Ctu2MBkYyMEF/jQnd"
                   };
        }
    }
    public class RequestChangePasswordDtoExample : IExamplesProvider<RequestChangePasswordDto>
    {
        public RequestChangePasswordDto GetExamples()
        {
            return new RequestChangePasswordDto()
                   {
                           NewPassword = "MyNewPassword#1234",
                           CurrentPassword = "MyPassword-1234"
                   };
        }
    }
    public class ChangedPasswordUserViewModelExample : IExamplesProvider<ChangedPasswordUserViewModel>
    {
        public ChangedPasswordUserViewModel GetExamples()
        {
            return new ChangedPasswordUserViewModel()
                   {
                           RefreshToken =
                                   "GfBT1FnxNEl5GhpahoZ8sI5L9m7LlkwoHtUyboFFWwdLC2KKtE+MfpnmWbK12sdkzJ6I1e9EzCaO/FneHQKKhNNsFZiXHreBE/o0XkUDqVPQjErk07IbLGm5z4IYXmeOrbkuRD0hYN9eYAAxj3QGh4tS8I7lo0hGsxZWigpMFLen2+GgrOwu/oZJC7zXvlSrcpbz1C32qh5ns9cJf0bLA8xB13aI8K0Ctu2MBkYyMiF/jRpi"
                   };
        }
    }
    #endregion

    #region Station Controller
    public class CreatedSessionViewModelExample : IExamplesProvider<CreatedSessionViewModel>
    {
        public CreatedSessionViewModel GetExamples()
        {
            return new CreatedSessionViewModel()
                   {

                           StationId = Guid.Parse("4bca67bf-cc6c-4c25-90d1-27ab5062d4dd"),
                           SessionId = Guid.NewGuid()
                   };
        }
    }
    public class UpdatedSessionViewModelModelExample : IExamplesProvider<UpdatedSessionViewModel>
    {
        public UpdatedSessionViewModel GetExamples()
        {
            return new UpdatedSessionViewModel()
                   {

                           StationId = Guid.Parse("4bca67bf-cc6c-4c25-90d1-27ab5062d4dd"),
                           SessionId = Guid.NewGuid(),
                           ChangeRequestId = Guid.NewGuid(),
                   };
        }
    }
    public class StoppedSessionViewModelExample : IExamplesProvider<StoppedSessionViewModel>
    {
        public StoppedSessionViewModel GetExamples()
        {
            return new StoppedSessionViewModel()
                   {

                           StationId = Guid.Parse("4bca67bf-cc6c-4c25-90d1-27ab5062d4dd"),
                           SessionId = Guid.NewGuid()
            };
        }
    }
    public class StationCurrentStateViewModelExample : IExamplesProvider<StationCurrentStateViewModel>
    {
        public StationCurrentStateViewModel GetExamples()
        {
            return new StationCurrentStateViewModel()
                   {
                           DisplayName = "Arcade One / Blue Room",
                           StationId = Guid.Parse("838c3a21-aacb-4012-8011-f20f8767f09f"),
                           ControlMode = StationControlMode.Local,
                           NetworkState = NetworkState.Connected,
                           Session = new SessionViewModel()
                                     {
                                             SessionId = Guid.NewGuid(),
                                             State = SessionState.Started,
                                             StartedOnUtc = DateTime.Parse("2019-06-07T03:32:07.064294Z"),
                                             MaxDurationLimit = TimeSpan.Parse("00:05:58.1770650")
                                     }

                   };
        }
    }
    public class StationCurrentStateViewModelMultipleExample : IExamplesProvider<IEnumerable<StationCurrentStateViewModel>>
    {
        public IEnumerable<StationCurrentStateViewModel> GetExamples()
        {
            return new List<StationCurrentStateViewModel>()
                   {
                           new StationCurrentStateViewModel()
                           {
                                   DisplayName = "Arcade One / Blue Room",
                                   StationId = Guid.Parse("838c3a21-aacb-4012-8011-f20f8767f09f"),
                                   ControlMode = StationControlMode.Local,
                                   NetworkState = NetworkState.Connected,
                                   Session = new SessionViewModel()
                                             {
                                                     SessionId = Guid.NewGuid(),
                                                     State = SessionState.Started,
                                                     StartedOnUtc = DateTime.Parse("2019-06-07T03:32:07.064294Z"),
                                                     MaxDurationLimit = TimeSpan.Parse("00:05:58.1770650")
                                             }

                           },
                           new StationCurrentStateViewModel()
                           {
                                   DisplayName = "Arcade Two / Blue Room",
                                   StationId = Guid.Parse("4bca67bf-cc6c-4c25-90d1-27ab5062d4dd"),
                                   ControlMode = StationControlMode.Remote,
                                   NetworkState = NetworkState.Connected,
                                   Session = new SessionViewModel()
                                             {
                                                     SessionId = Guid.NewGuid(),
                                                     Reference = "N-C:0007-P:0.50-D:00-05-00",
                                                     State = SessionState.Started,
                                                     StartedOnUtc = DateTime.Parse("2019-06-07T03:50:56.913479Z"),
                                                     StartDuration = TimeSpan.Parse("00:05:00"),
                                                     MaxDurationLimit = TimeSpan.Parse("00:10:00")
                                             }

                           }
                   };
        }
    }
    public class SessionLogViewModelMultipleExample : IExamplesProvider<IEnumerable<SessionLogViewModel>>
    {
        public IEnumerable<SessionLogViewModel> GetExamples()
        {
            return new List<SessionLogViewModel>()
                   {
                           new SessionLogViewModel()
                           {
                                   StationId = Guid.Parse("838c3a21-aacb-4012-8011-f20f8767f09f"),
                                   SessionId = Guid.NewGuid(),
                                   RequestedBy = RequestSource.WebApi,
                                   LatestState = SessionState.Ended,
                                   Reference = "Your reference would be here",
                                   StartedUtc = DateTime.Parse("2019-06-04T11:22:28.242865Z"),
                                   EndedUtc = DateTime.Parse("2019-06-04T12:47:17.0573Z"),
                                   StoppedBy = StopReason.RemoteLogout,
                           },
                           new SessionLogViewModel()
                           {
                                   StationId = Guid.Parse("838c3a21-aacb-4012-8011-f20f8767f09f"),
                                   SessionId = Guid.NewGuid(),
                                   RequestedBy = RequestSource.WebApi,
                                   LatestState = SessionState.Ended,
                                   Reference = "NID:14-C:0007-P:0.50-D:00-05-00",
                                   StartedUtc = DateTime.Parse("2019-06-07T06:25:04.465401Z"),
                                   MaxDurationLimit = TimeSpan.Parse("00:08:30"),
                                   EndedUtc = DateTime.Parse("2019-06-07T06:33:34.001363Z"),
                                   StoppedBy = StopReason.LimitReached,
                                   ChangeRequests = new List<ChangeRequestViewModel>()
                                                    {
                                                            new ChangeRequestViewModel()
                                                            {
                                                                    Id = Guid.NewGuid(),
                                                                    CreatedOnUtc = DateTime.Parse("2019-06-07T06:27:58.497276Z"),
                                                                    Reference  = "UID:01-CID:0007-PAY:1.00USD-DUR:00-02-00",
                                                                    TimeChange = TimeSpan.Parse("00:02:00")
                                                            },
                                                            new ChangeRequestViewModel()
                                                            {
                                                                    Id = Guid.NewGuid(),
                                                                    CreatedOnUtc = DateTime.Parse("2019-06-07T06:28:31.373925Z"),
                                                                    Reference  = "UID:02-CID:0007-PAY:0.50USD-DUR:00-01-00",
                                                                    TimeChange = TimeSpan.Parse("00:01:00")
                                                            },
                                                            new ChangeRequestViewModel()
                                                            {
                                                                    Id = Guid.NewGuid(),
                                                                    CreatedOnUtc = DateTime.Parse("2019-06-07T06:32:57.088249Z"),
                                                                    Reference  = "ID:03-CID:0007-PAY:0.25USD-DUR:00-00-30",
                                                                    TimeChange = TimeSpan.Parse("00:00:30")
                                                            }
                                                    }
                           }
                   };
        }
    }
    public class StationSettingsViewModelMultipleExample : IExamplesProvider<IEnumerable<StationSettingsViewModel>>
    {
        public IEnumerable<StationSettingsViewModel> GetExamples()
        {
            return new List<StationSettingsViewModel>
                   {
                           new StationSettingsViewModel()
                           {
                                   DisplayName = "Arcade One / Blue Room",
                                   StationId = Guid.Parse("838c3a21-aacb-4012-8011-f20f8767f09f"),
                                   ControlMode = StationControlMode.Local,
                           },
                           new StationSettingsViewModel()
                           {
                                   DisplayName = "Arcade One / Red Room",
                                   StationId = Guid.Parse("56f132c8-51dc-44b2-9bd9-101153fa5832"),
                                   ControlMode = StationControlMode.Remote
                           }
                           ,
                           new StationSettingsViewModel()
                           {
                                   DisplayName = "Arcade Two / Red Room",
                                   StationId = Guid.Parse("56f132c8-51dc-44b2-9bd9-101153fa5832"),
                                   ControlMode = StationControlMode.RemoteWithQrCode,
                                   QrCode =
                                           @"http://api.myarcade.com/Station/56f132c8-51dc-44b2-9bd9-101153fa5832/Session&request=start&myappToken={appToken}"
                           },
                   };
        }
    }
    public class StationSettingsViewModelExample : IExamplesProvider<StationSettingsViewModel>
    {
        public StationSettingsViewModel GetExamples()
        {
            return new StationSettingsViewModel()
                   {
                           DisplayName = "Arcade One / Blue Room",
                           StationId = Guid.Parse("838c3a21-aacb-4012-8011-f20f8767f09f"),
                           ControlMode = StationControlMode.RemoteWithQrCode,
                           QrCode =
                                   @"http://api.myarcade.com/Station/56f132c8-51dc-44b2-9bd9-101153fa5832/Session&request=start&myappToken={appToken}"
            };
        }
    }

    public class StationApiKeyViewModelExample : IExamplesProvider<StationApiKeyViewModel>
    {
        public StationApiKeyViewModel GetExamples()
        {
            return new StationApiKeyViewModel()
                   {
                           CreateOnUtc = DateTime.UtcNow,
                           Name = "Station01-CoinAcceptor",
                           PublicKey = "1feb88acdac441be928f995edd41a8f8",
                           Secret = "A0Sw8Xh+uclICOUPM/OmaUKK+Px1F0kazZPqFy71RY5="
                   };
        }
    }

    public class StationApiKeyViewModelMultipleExample : IExamplesProvider<IEnumerable<StationApiKeyViewModel>>
    {
        public IEnumerable<StationApiKeyViewModel> GetExamples()
        {
            return new[]
                   {
                           new StationApiKeyViewModel()
                           {
                                   CreateOnUtc = DateTime.UtcNow,
                                   Name = "Station01-CoinAcceptor",
                                   PublicKey = "1feb88acdac441be928f995edd41a8f8",
                                   Secret = "A0Sw8Xh+uclICOUPM/OmaUKK+Px1F0kazZPqFy71RY5="
                           },
                           new StationApiKeyViewModel()
                           {
                                   CreateOnUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(3.432)),
                                   Name = "Station01-RemotePanel",
                                   PublicKey = "7010e93e3e68430087dc421743c83448",
                                   Secret = "bY34TLww9gdg6CZFY5UPDpChPoxW7nxGdd3HCn/Zkpg="
                           }
                   };
        }
    }
    public class RequestStationSettingsDtoExample : IExamplesProvider<RequestStationSettingsDto>
    {
        public RequestStationSettingsDto GetExamples()
        {
            return new RequestStationSettingsDto()
                   {
                           DisplayName = "Arcade One / Blue Room",
                           Mode = StationControlMode.RemoteWithQrCode,
                           QrCode =
                                   @"http://api.myarcade.com/Station/56f132c8-51dc-44b2-9bd9-101153fa5832/Session&request=start&myappToken={appToken}",

                   };
        }
    }
    public class RequestSessionUpdateDtoExample : IExamplesProvider<RequestSessionUpdateDto>
    {
        public RequestSessionUpdateDto GetExamples()
        {
            return new RequestSessionUpdateDto()
                   {
                           Reference = "UID:01-CID:0007-PAY:1.00USD-DUR:00-01-30",
                           Duration = TimeSpan.FromSeconds(90)

                   };
        }
    }
    public class RequestNewStationSessionDtoExample : IExamplesProvider<RequestNewStationSessionDto>
    {
        public RequestNewStationSessionDto GetExamples()
        {
            return new RequestNewStationSessionDto()
                   {
                           Reference = "NID:14-C:0007-P:0.50-D:00-05-00",
                           Duration = null,
                   };
        }

    }

    public class RequestStationModeDtoExample : IExamplesProvider<RequestStationModeDto>
    {
        public RequestStationModeDto GetExamples()
        {
            return new RequestStationModeDto()
                   {
                           Mode = StationControlMode.Local
                   };
        }
    }

    public class RequestSetStationPasswordDtoExample : IExamplesProvider<RequestSetStationPasswordDto>
    {
        public RequestSetStationPasswordDto GetExamples()
        {
            return new RequestSetStationPasswordDto()
                   {
                           Password = "SecurePassword#_0001"
                   };
        }
    }

    public class RequestStationQrCodeDtoExample : IExamplesProvider<RequestStationQrCodeDto>
    {
        public RequestStationQrCodeDto GetExamples()
        {
            return new RequestStationQrCodeDto
                   {
                           QrCode = "https://api.company.com/leapplay/838c3a21-aacb-4012-8011-f20f8767f09f?customer={customerId}&app={appId}"
                   };
        }
    }
    #endregion

    public class ResponseErrorExample : IExamplesProvider<IDictionary<string, string[]>>
    {
        public IDictionary<string, string[]> GetExamples()
        {
            return new Dictionary<string, string[]>()
                   {
                           {
                                   "errorTag",
                                   new string[]
                                   {
                                           "First Error Detail found under this tag",
                                           "Second Error Details found under this tag"
                                   }
                           }
                   };
        }
    }
}