import { useCallback, useEffect, useRef, useState } from "react";

export function useFetch<T>(
  fetcher: () => Promise<T>,
  deps: unknown[] = [],
) {
  const [data, setData] = useState<T | null>(null);
  const [isPending, setIsPending] = useState(true);

  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  const refetch = useCallback(async () => {
    const result = await fetcherRef.current();
    setData(result);
    return result;
  }, []);

  useEffect(() => {
    let isCancelled = false;
    setIsPending(true);
    fetcherRef.current()
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
