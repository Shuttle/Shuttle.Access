import moment from "moment";

export function useDateFormatter(value: string) {
  return moment(value).format("yyyy-MM-DD");
}

export function useDateTimeFormatter(value: string) {
  return moment(value).format("yyyy-MM-DD HH:mm:ss");
}
