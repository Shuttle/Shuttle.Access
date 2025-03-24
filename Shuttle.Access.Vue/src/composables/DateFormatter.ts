export interface DateTimeFormatter {
  date(): string | null;
  dateTime(): string | null;
  dateTimeMilliseconds(): string | null;
  isoDate(): string | null;
  isoDateTime(): string | null;
}

export function useDateFormatter(
  value: Date | string | null | undefined,
): DateTimeFormatter {
  if (!value) {
    return {
      date() {
        return null;
      },
      dateTime() {
        return null;
      },
      dateTimeMilliseconds() {
        return null;
      },
      isoDate() {
        return null;
      },
      isoDateTime() {
        return null;
      },
    };
  }

  const dt = new Date(value);

  const year = dt.getFullYear();
  const month = String(dt.getMonth() + 1).padStart(2, "0");
  const day = String(dt.getDate()).padStart(2, "0");
  const hours = String(dt.getHours()).padStart(2, "0");
  const minutes = String(dt.getMinutes()).padStart(2, "0");
  const seconds = String(dt.getSeconds()).padStart(2, "0");
  const ms = String(dt.getMilliseconds()).padStart(3, "0");

  return {
    date() {
      return `${year}/${month}/${day}`;
    },
    dateTime() {
      return `${year}/${month}/${day} ${hours}:${minutes}:${seconds}`;
    },
    dateTimeMilliseconds() {
      return `${year}/${month}/${day} ${hours}:${minutes}:${seconds}.${ms}`;
    },
    isoDate() {
      return `${year}-${month}-${day}`;
    },
    isoDateTime() {
      return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}Z`;
    },
  };
}
