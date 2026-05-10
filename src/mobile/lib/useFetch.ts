import { useCallback, useEffect, useState } from "react";

export function useFetch<T>(
  fetcher: () => Promise<T>,
  deps: unknown[] = [],
) {
  const [data, setData] = useState<T | null>(null);
  const [isPending, setIsPending] = useState(true);

  const refetch = useCallback(async () => {
    const result = await fetcher();
    setData(result);
    return result;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  useEffect(() => {
    let isCancelled = false;
    setIsPending(true);
    fetcher()
      .then((result) => {
        if (!isCancelled) setData(result);
      })
      .finally(() => {
        if (!isCancelled) setIsPending(false);
      });
    return () => {
      isCancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  return { data, isPending, refetch };
}
