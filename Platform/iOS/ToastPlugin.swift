import UIKit

@MainActor final class ToastPlugin {
    private static var instance: ToastPlugin?
    static func register() {
        if instance == nil {
            instance = ToastPlugin()
        }
    }

    private init() {
        let channel = NativeChannels.channel("dotnative.toast")
        channel.handle("show") {
            [weak self] arguments, reply in
            self?.show(arguments, reply)
                ?? reply.failure("unavailable", "Toast plugin is unavailable")
        }
    }

    private func show(_ arguments: PluginValue, _ reply: PluginReply) {
        let fields = arguments.fields
        guard let message = fields["message"]?.string, !message.isEmpty, message.count <= 2_000,
            let window = NativeChannels.presenter?.view.window
        else {
            reply.failure("invalid_argument", "A message and an active window are required")
            return
        }
        ToastOverlay.show(message, in: window)
        reply.success()
    }
}

@MainActor private final class ToastOverlay: UIView {
    static func show(_ message: String, in window: UIWindow) {
        let label = UILabel()
        label.text = message
        label.font = .systemFont(ofSize: 16)
        label.textColor = UIColor(white: 128 / 255, alpha: 1)
        label.numberOfLines = 8
        label.lineBreakMode = .byTruncatingTail
        let maximum = CGSize(width: window.bounds.width * 0.8, height: window.bounds.height * 0.8)
        let measured = label.sizeThatFits(maximum)
        let size = CGSize(
            width: min(measured.width, maximum.width), height: min(measured.height, maximum.height)
        )
        let toast = ToastOverlay(
            frame: CGRect(origin: .zero, size: CGSize(width: size.width + 20, height: size.height + 20))
        )
        label.frame = CGRect(origin: CGPoint(x: 10, y: 10), size: size)
        toast.addSubview(label)
        toast.backgroundColor = UIColor(white: 246 / 255, alpha: 1)
        toast.layer.cornerRadius = 10
        toast.autoresizingMask = [
            .flexibleLeftMargin, .flexibleRightMargin, .flexibleTopMargin, .flexibleBottomMargin,
        ]
        toast.center = CGPoint(x: window.bounds.midX, y: window.bounds.midY)
        toast.alpha = 0
        toast.addGestureRecognizer(UITapGestureRecognizer(target: toast, action: #selector(dismiss)))
        window.addSubview(toast)
        UIView.animate(withDuration: 0.2, delay: 0, options: [.curveEaseOut, .allowUserInteraction])
        {
            toast.alpha = 1
        } completion: { _ in
            DispatchQueue.main.asyncAfter(deadline: .now() + 2) { toast.dismiss() }
        }
    }

    @objc private func dismiss() {
        UIView.animate(withDuration: 0.2, delay: 0, options: [.curveEaseIn, .beginFromCurrentState])
        {
            self.alpha = 0
        } completion: { _ in
            self.removeFromSuperview()
        }
    }
}
