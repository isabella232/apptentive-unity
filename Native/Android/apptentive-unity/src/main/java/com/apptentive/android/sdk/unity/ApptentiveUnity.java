package com.apptentive.android.sdk.unity;

import android.app.Activity;
import android.app.Application;
import android.util.Log;

import com.apptentive.android.sdk.Apptentive;
import com.apptentive.android.sdk.ApptentiveConfiguration;
import com.apptentive.android.sdk.ApptentiveLog;
import com.apptentive.android.sdk.lifecycle.ApptentiveActivityLifecycleCallbacks;
import com.apptentive.android.sdk.module.messagecenter.UnreadMessagesListener;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

import static com.unity3d.player.UnityPlayer.currentActivity;

public final class ApptentiveUnity {
	private static final String TAG = ApptentiveUnity.class.getSimpleName();

	private static final String CALLBACK_NAME_BOOLEAN = "booleanCallback";
	private static final String CALLBACK_NAME_UNREAD_MESSAGE_COUNT_CHANGED = "unreadMessageCountChanged";

	private static String scriptTarget;
	private static String scriptMethod;

	// we have to keep a strong reference to the listener since the SDK doesn't keep it
	private static UnreadMessagesListener unreadMessagesListener = new UnreadMessagesListener() {
		@Override
		public void onUnreadMessageCountChanged(int unreadMessages) {
			Map<String, Object> payload = new HashMap<>();
			payload.put("unreadMessages", unreadMessages);
			sendNativeCallback(CALLBACK_NAME_UNREAD_MESSAGE_COUNT_CHANGED, payload);
		}
	};

	public static boolean register(String target, String method, String version, String configurationJson) {
		try {
			scriptTarget = target;
			scriptMethod = method;

			ApptentiveConfiguration configuration = parseConfiguration(configurationJson);
			Apptentive.register(getApplication(), configuration);
			Apptentive.addUnreadMessagesListener(unreadMessagesListener);

			Application.ActivityLifecycleCallbacks callbacks = getActivityLifecycleCallbacks();
			callbacks.onActivityCreated(getActivity(), null);
			callbacks.onActivityStarted(getActivity());
			callbacks.onActivityResumed(getActivity());
			return true;
		} catch (Exception e) {
			ApptentiveLog.e(e, "Exception while registering Apptentive");
		}

		return false;
	}

	public static int showMessageCenter(String customData) {
		BooleanIdCallback callback = new BooleanIdCallback();
		Apptentive.showMessageCenter(getActivity(), callback);
		return callback.getId();
	}

	public static int canShowMessageCenter() {
		BooleanIdCallback callback = new BooleanIdCallback();
		Apptentive.canShowMessageCenter(callback);
		return callback.getId();
	}

	public static int getUnreadMessageCount() {
		return Apptentive.getUnreadMessageCount();
	}

	public static int engage(String event, String customData) {
		BooleanIdCallback callback = new BooleanIdCallback();
		Apptentive.engage(getActivity(), event, callback);
		return callback.getId();
	}

	public static int queryCanShowInteraction(String event) {
		BooleanIdCallback callback = new BooleanIdCallback();
		Apptentive.queryCanShowInteraction(event, callback);
		return callback.getId();
	}

	public static void setPersonName(String name) {
		Apptentive.setPersonName(name);
	}

	public static void setPersonEmail(String email) {
		Apptentive.setPersonEmail(email);
	}

	//region Configuration

	private static ApptentiveConfiguration parseConfiguration(String json) throws JSONException {
		JSONObject configurationJson = new JSONObject(json);
		String apptentiveKey = configurationJson.getString("apptentiveKey");
		String apptentiveSignature = configurationJson.getString("apptentiveSignature");

		ApptentiveConfiguration configuration = new ApptentiveConfiguration(apptentiveKey, apptentiveSignature);

		// log level
		String logLevelString = configurationJson.optString("logLevel");
		ApptentiveLog.Level logLevel = parseLogLevel(logLevelString);
		if (logLevel != ApptentiveLog.Level.UNKNOWN) {
			configuration.setLogLevel(logLevel);
		}

		// should sanitize log level
		boolean shouldSanitizeLogMessages = configurationJson.optBoolean("shouldSanitizeLogMessages", true);
		configuration.setShouldSanitizeLogMessages(shouldSanitizeLogMessages);

		return configuration;
	}

	private static ApptentiveLog.Level parseLogLevel(String value) {
		switch (value) {
			case "Verbose":
				return ApptentiveLog.Level.VERBOSE;
			case "Debug":
				return ApptentiveLog.Level.DEBUG;
			case "Info":
				return ApptentiveLog.Level.INFO;
			case "Warn":
				return ApptentiveLog.Level.WARN;
			case "Error":
				return ApptentiveLog.Level.ERROR;
		}

		return ApptentiveLog.Level.UNKNOWN;
	}

	//endregion

	//region Callbacks

	private static class BooleanIdCallback implements Apptentive.BooleanCallback {
		private static int nextId;

		private final int id;

		public BooleanIdCallback() {
			this.id = getNextId();
		}

		@Override
		public void onFinish(boolean result) {
			Map<String, Object> payload = new HashMap<>();
			payload.put("id", id);
			payload.put("result", result);
			sendNativeCallback(CALLBACK_NAME_BOOLEAN, payload);
		}

		public int getId() {
			return id;
		}

		private synchronized static int getNextId() {
			return ++nextId;
		}
	}

	private static void sendNativeCallback(String name, Map<String, Object> payload) {
		try {
			String data = serialize(name, payload);
			UnityPlayer.UnitySendMessage(scriptTarget, scriptMethod, data);
		} catch (Exception e) {
			Log.e(TAG, "Exception while sending native callback", e);
		}
	}

	private static String serialize(String name, Map<String, Object> payload) throws JSONException {
		JSONObject json = new JSONObject();
		json.put("name", name);
		json.put("payload", new JSONObject(payload));
		return json.toString();
	}

	//endregion

	//region Helpers

	private static Application getApplication() {
		return getActivity().getApplication();
	}

	private static Activity getActivity() {
		return currentActivity;
	}

	private static Application.ActivityLifecycleCallbacks getActivityLifecycleCallbacks() {
		return ApptentiveActivityLifecycleCallbacks.getInstance();
	}

	//endregion
}
