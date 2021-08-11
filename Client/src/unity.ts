interface UnityInstance {
  SetFullscreen(fullscreen: 0 | 1): void;
  SendMessage(
    objectName: string,
    methodName: string,
    value?: string | number
  ): void;
  Quit(): Promise<void>;
}

interface UnityConfig {
  dataUrl: string;
  frameworkUrl: string;
  codeUrl: string;
  streamingAssetsUrl: string;
  companyName: string;
  productName: string;
  productVersion: string;
  devicePixelRatio?: number;
}

interface UnityCreator {
  (
    canvas: HTMLCanvasElement,
    config: any,
    progressCallback: (progress: number) => void
  ): Promise<UnityInstance>;
}

function GetUnityLoader() {
  return {
    get createUnityInstance() {
      return <UnityCreator>(<any>globalThis).createUnityInstance;
    },
    get isValid() {
      return !!(<any>globalThis).createUnityInstance;
    }
  };
}
