// -------------------------------------------------------------------------
//    @FileName			:    NFCGameScene.h
//    @Author           :    Johance
//    @Date             :    2016-12-28
//    @Module           :    NFCGameScene
//
// -------------------------------------------------------------------------
#ifndef NFCGameScene_H
#define NFCGameScene_H

#include "NFCSceneManager.h"

class NFCGameScene : public IUniqueScene<NFCGameScene>
{
public:
	NFCGameScene();
	~NFCGameScene();
	
	virtual bool initLayout();
	virtual void initData(void *pData);
	virtual void update(float dt);

	virtual bool onTouchBegan(Touch *touch, Event *unused_event);
	virtual void onTouchMoved(Touch *touch, Event *unused_event);
private:
	int OnObjectClassEvent(const NFGUID& self, const std::string& strClassName, const CLASS_OBJECT_EVENT eClassEvent, const NFIDataList& var);
	int OnObjectPropertyEvent(const NFGUID& self, const std::string& strPropertyName, const NFIDataList::TData& oldVar, const NFIDataList::TData& newVar);
	int OnPlayerMoveEvent(const int nEventID, const NFIDataList& varDataList);
	int OnPlayerChatEvent(const int nEventID, const NFIDataList& varDataList);

private:
	NFMap<NFGUID, Sprite> m_Players;
};

#endif // __HELLOWORLD_SCENE_H__
