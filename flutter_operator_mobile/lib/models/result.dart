/// Generic success/error result wrapper. Mirrors the IResult<T> pattern the
/// server uses on the .NET side so the Flutter code keeps the same vocabulary.
class Result<T> {
  final T? value;
  final List<String> errors;

  const Result.ok(T this.value) : errors = const [];
  const Result.error(this.errors) : value = null;

  factory Result.errorFromString(String msg) => Result.error([msg]);
  factory Result.errorFromException(Object e) => Result.error([e.toString()]);

  bool get isSuccess => errors.isEmpty;
  bool get hasError => errors.isNotEmpty;
}
