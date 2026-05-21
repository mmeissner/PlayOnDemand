import 'package:flutter/material.dart';

/// Single source of truth for the legacy `leap_play_x_app` PNG assets. Each
/// icon is sized to its intended UI usage with `Image.asset`; the asset files
/// themselves are 512x512 black-on-transparent and scale crisply.
class AppIcons {
  static const _dir = 'assets/images';

  // brand
  static const String logo = '$_dir/leap_play_header_logo.jpg';
  static const String mountainsBackdrop = '$_dir/mountains.jpg';

  // generic
  static const String station = '$_dir/station.png';
  static const String server = '$_dir/server_icon.png';
  static const String statistics = '$_dir/statistics_icon.png';
  static const String support = '$_dir/support_icon.png';
  static const String wrench = '$_dir/wrench_icon.png';
  static const String gear = '$_dir/gear_icon.png';
  static const String menuGear = '$_dir/gear_Menue_icon.png';
  static const String logout = '$_dir/logout_icon.png';

  // station state
  static const String stationDisconnect = '$_dir/station_disconnect_icon.png';
  static const String stationIdle = '$_dir/station_idle_icon.png';
  static const String stationSession = '$_dir/station_session_icon.png';

  // control modes
  static const String modeLocal = '$_dir/couch_icon.png';
  static const String modeRemote = '$_dir/remote_icon.png';
  static const String modeQrCode = '$_dir/qrcode_icon.png';

  // session actions
  static const String play = '$_dir/play_icon.png';
  static const String stop = '$_dir/stop_icon.png';
  static const String updateSession = '$_dir/update_session_icon.png';

  /// Returns the right station-state PNG for the given networkState +
  /// hasSession combo. Used on the stations list + per-station screens.
  static String stationStateAsset({
    required bool isConnected,
    required bool hasSession,
  }) {
    if (!isConnected) return stationDisconnect;
    if (hasSession) return stationSession;
    return stationIdle;
  }

  static String controlModeAsset(String mode) {
    switch (mode) {
      case 'Remote':
        return modeRemote;
      case 'RemoteWithQrCode':
        return modeQrCode;
      case 'Local':
      default:
        return modeLocal;
    }
  }

  /// `Image.asset` shorthand with sensible square sizing.
  static Widget img(String asset, {double size = 36, Color? color}) {
    return Image.asset(
      asset,
      width: size,
      height: size,
      color: color,
      colorBlendMode: color == null ? null : BlendMode.srcIn,
    );
  }
}
