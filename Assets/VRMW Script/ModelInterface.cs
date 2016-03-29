using UnityEngine;
using System.Collections;

public interface ModelInterface {

	void attack (Transform target, int user);
	void damaged (int user);
}
